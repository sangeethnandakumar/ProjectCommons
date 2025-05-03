> Base Model - The base odel used here is `google/flan-t5-small`
> Base Model Location - The initial model downloaded will be placed in `%USERPROFILE%\.cache\huggingface\hub`
> Fine Tuned Model Location - The final fine tuned model will be on `./fine_tuned_model`

<hr/>

## Fine Tuned Output Model
Key output files:
- model.safetensors → Your fine-tuned model weights
- config.json → Model architecture config
- tokenizer.json, tokenizer_config.json, etc. → Tokenizer files
- generation_config.json → Inference defaults (optional)
- checkpoint-* folders → Intermediate training snapshots (can be deleted if not resuming training)

<hr/>

# Collect High Quality Data
If data isn't available, The below ChatGPT prompt can be used to generate data
```txt
Generate 1000 item csv dataset with 3 string columns "instruction", "input" & "output".
Ensure only output is pipe delimited and overall file is comma delimited

Sample Row:
"instruction","input","output"
"EXTRACT BANK SMS:","ICICI Bank Acct XX123 debited for Rs 123.45 on 13-Apr-25; NAME credited. UPI:123456789012. Call 18001234 for dispute. SMS BLOCK 123 to 9215000000","UPI|123456789012|123.45|Rs|NAME|ICICI Bank|2025-04-13 12:00AM"

Here are some examples of various inputs:
ICICI & Federal Bank variations. UPI vs Credit card modes
------------------------------------------------------------
- ICICI Bank Acct XX123 debited for Rs 123.45 on 13-Apr-25; NAME credited. UPI:123456789012. Call 18001234 for dispute. SMS BLOCK 123 to 9215000000
- INR 23.25 spent using ICICI Bank Card XX1234 on 03-May-25 on MERCHANT NAME. Avl Limit: INR 45781.24. If not you, call 18001234/SMS BLOCK 1234 to 9215000000
- INR 23.25 spent using ICICI Bank Card XX1234 on 03-May-25 on MERCHANT NAME. Avl Limit: INR 45781.24. If not you, call 18001234/SMS BLOCK - 1234 to 9215000000
- Rs 359.00 debited via UPI on 02-05-2025 14:58:09 to VPA vpa1@fbl.Ref No 123456789012.Small txns?Use UPI Lite!-Federal Bank
- Rs 40.00 debited via UPI on 02-05-2025 11:14:03 to VPA vpa2@ptys.Ref No 123456789012.Small txns?Use UPI Lite!-Federal Bank
```

# dataset.csv
This generates a CSV data like below
![image](https://github.com/user-attachments/assets/d8716999-f97a-43fc-84ec-28cb747d6d5a)

# build-dataset.py ==> train.jsonl
Generate JSON training data from this CSV file
```py
import pandas as pd
import json

def convert_csv_to_jsonl(csv_path, output_path):
    df = pd.read_csv(csv_path)
    with open(output_path, "w", encoding="utf-8") as f:
        for _, row in df.iterrows():
            sample = {
                "instruction": row["instruction"],
                "input": row["input"],
                "output": row["output"]
            }
            f.write(json.dumps(sample, ensure_ascii=False) + "\n")
    print("✅ Converted 'dataset.csv' ==> 'train.jsonl'")

if __name__ == '__main__':
    convert_csv_to_jsonl("dataset.csv", "train.jsonl")
```
![image](https://github.com/user-attachments/assets/1d025fd1-88f7-4a1c-9e6b-700cbae5ed94)

![image](https://github.com/user-attachments/assets/68cb1b54-1a5c-4708-928e-1e3e91552480)

# deep-fine-tune.py
Now the `train.jsonl` can be use to fine tune the base model.

I'm using a small `80M parameter` distil model by Google `google/flan-t5-small` from Huggingface
https://huggingface.co/google/flan-t5-small#model-details

```py
import torch
from transformers import (
    AutoTokenizer,
    AutoModelForSeq2SeqLM,
    TrainingArguments,
    Trainer,
    DataCollatorForSeq2Seq
)
from datasets import load_dataset

# Configuration
MODEL_ID = "google/flan-t5-small"
DATASET_PATH = "train.jsonl"  # Your JSON file
OUTPUT_DIR = "./fine_tuned_model"

# Training parameters (optimized for CPU)
BATCH_SIZE = 80
EPOCHS = 5
LEARNING_RATE = 3e-4
MAX_INPUT_LENGTH = 128
MAX_OUTPUT_LENGTH = 128  # Increased for JSON output

# Load model and tokenizer
print("📥 Loading model...")
tokenizer = AutoTokenizer.from_pretrained(MODEL_ID)
model = AutoModelForSeq2SeqLM.from_pretrained(MODEL_ID)

# Load raw data without validation
print("🗂️ Loading raw dataset...")
dataset = load_dataset("json", data_files=DATASET_PATH)["train"].train_test_split(test_size=0.1)


# Tokenization (direct processing)
def tokenize_function(examples):
    # Combine instruction + input
    inputs = [f"{inst} {inp}" for inst, inp in zip(examples["instruction"], examples["input"])]

    # Tokenize inputs
    model_inputs = tokenizer(
        inputs,
        max_length=MAX_INPUT_LENGTH,
        truncation=True,
        padding="max_length"
    )

    # Tokenize JSON outputs
    model_inputs["labels"] = tokenizer(
        examples["output"],
        max_length=MAX_OUTPUT_LENGTH,
        truncation=True,
        padding="max_length"
    )["input_ids"]

    return model_inputs


# Process dataset
print("🔢 Tokenizing...")
tokenized_dataset = dataset.map(
    tokenize_function,
    batched=True,
    remove_columns=["instruction", "input", "output"]  # Remove original fields
)

# Training setup
training_args = TrainingArguments(
    output_dir=OUTPUT_DIR,
    per_device_train_batch_size=BATCH_SIZE,
    num_train_epochs=EPOCHS,
    learning_rate=LEARNING_RATE,
    eval_strategy="epoch",
    save_strategy="epoch",
    remove_unused_columns=False,
    use_cpu=True,  # CPU-only
    fp16=False,
    logging_steps=10
)

# Trainer
trainer = Trainer(
    model=model,
    args=training_args,
    train_dataset=tokenized_dataset["train"],
    eval_dataset=tokenized_dataset["test"],
    data_collator=DataCollatorForSeq2Seq(tokenizer, model=model)
)

# Start training
print("🚀 Training...")
trainer.train()

# Save model
model.save_pretrained(OUTPUT_DIR)
tokenizer.save_pretrained(OUTPUT_DIR)
print(f"💾 Saved to {OUTPUT_DIR}")
```

### (In Above Code) Adjust the training parameters above to match system resources
```py
# Training parameters (optimized for CPU)
BATCH_SIZE = 60  # Adjust based on RAM
EPOCHS = 10  # Adjust based on time
LEARNING_RATE = 3e-4
MAX_INPUT_LENGTH = 128  # Adjust based on input
MAX_OUTPUT_LENGTH = 80  # Adjust based on output
```

![image](https://github.com/user-attachments/assets/7eec158c-9e61-40dc-872d-da49fd686da5)

# evaluvate-batch.py
This reads data from below `verify.txt` and tries and get the result
![image](https://github.com/user-attachments/assets/d949729f-3980-4fe2-846b-737f47d3c549)

![image](https://github.com/user-attachments/assets/4480b627-1c80-4b6b-9308-baf2402aa7fb)

```py
import warnings
from transformers import AutoTokenizer, AutoModelForSeq2SeqLM, pipeline
from transformers.utils import logging

# Suppress all warnings
warnings.filterwarnings("ignore")
logging.set_verbosity_error()

MODEL_PATH = "./fine_tuned_model"
MAX_OUTPUT_LENGTH = 128
INPUT_FILE = "verify.txt"

def generate_response(pipe, text):
    return pipe(
        text,
        max_length=MAX_OUTPUT_LENGTH,
        num_beams=4,
        early_stopping=True,
        temperature=0.7,
        no_repeat_ngram_size=2,
        do_sample=True
    )[0]["generated_text"]

def print_separator():
    print("-" * 80)

def process_batch(generator):
    try:
        with open(INPUT_FILE, 'r', encoding='utf-8') as file:
            prompts = [line.strip() for line in file if line.strip()]
    except FileNotFoundError:
        print(f"❌ Error: {INPUT_FILE} not found in current directory")
        return

    if not prompts:
        print(f"ℹ️ {INPUT_FILE} is empty")
        return

    print(f"📁 Processing {len(prompts)} prompts from {INPUT_FILE}")
    print_separator()

    for i, prompt in enumerate(prompts, 1):
        try:
            response = generate_response(generator, prompt)
            print(f"Prompt #{i}: {prompt}")
            print(f"Response: {response}\n")
            print_separator()
        except Exception as e:
            print(f"⚠️ Error processing prompt #{i}: {e}")
            print_separator()

def main():
    print("🤖 Loading model...")
    tokenizer = AutoTokenizer.from_pretrained(MODEL_PATH)
    model = AutoModelForSeq2SeqLM.from_pretrained(MODEL_PATH)

    generator = pipeline(
        "text2text-generation",
        model=model,
        tokenizer=tokenizer,
        device=-1
    )

    print("✅ Model ready")
    print_separator()

    process_batch(generator)
    print("Batch processing complete")

if __name__ == "__main__":
    main()
```

# evaluvate-realtime.py
This allows running realtime prompts to the model

![image](https://github.com/user-attachments/assets/5e0c0ad9-e29b-4773-85ba-c45b1607fbeb)

```py
import warnings
from transformers import AutoTokenizer, AutoModelForSeq2SeqLM, pipeline
from transformers.utils import logging

# Suppress all warnings
warnings.filterwarnings("ignore")
logging.set_verbosity_error()

MODEL_PATH = "./fine_tuned_model"
MAX_OUTPUT_LENGTH = 128


def generate_response(pipe, text):
    return pipe(
        text,
        max_length=MAX_OUTPUT_LENGTH,
        num_beams=4,
        early_stopping=True,
        temperature=0.7,
        no_repeat_ngram_size=2,
        do_sample=True  # Added to prevent temperature warning
    )[0]["generated_text"]


def print_separator():
    print("-" * 80)


def main():
    print("🤖 Loading model...")
    tokenizer = AutoTokenizer.from_pretrained(MODEL_PATH)
    model = AutoModelForSeq2SeqLM.from_pretrained(MODEL_PATH)

    generator = pipeline(
        "text2text-generation",
        model=model,
        tokenizer=tokenizer,
        device=-1
    )

    print("✅ Model ready")
    print_separator()

    while True:
        try:
            user_input = input("Prompt: ")

            if user_input.lower() in ['exit', 'quit', 'q']:
                print("\nGoodbye!")
                break

            if not user_input.strip():
                continue

            response = generate_response(generator, user_input)
            print(f"Response: {response}")
            print_separator()

        except KeyboardInterrupt:
            print("\nGoodbye!")
            break
        except Exception as e:
            print(f"Error: {e}")
            print_separator()
            continue


if __name__ == "__main__":
    main()
```
