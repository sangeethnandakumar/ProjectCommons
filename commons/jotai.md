# Jotai State Management Example

Jotai is a modern state management library for React that enables you to create and manage a single source of truth for your application state. Below is a concise guide to meet your requirements.

## 1. Single Source of Truth JSON Object

To create a single source of truth, use a Jotai atom to hold the central JSON object.

### Code Example

```jsx
// atoms.js
import { atom } from 'jotai';

// Central JSON object state
export const jsonStateAtom = atom({
    key1: 'value1',
    key2: 'value2',
    key3: 'value3',
});
```

This atom will serve as the single source of truth for your app.

## 2. Modify State and Reflect Changes

You can modify the Jotai atom's value from any component, and the updates will immediately reflect across the app.

### Code Example

#### Parent Component
```jsx
// App.js
import React from 'react';
import { Provider } from 'jotai';
import DisplayComponent from './DisplayComponent';
import ModifyComponent from './ModifyComponent';

function App() {
    return (
        <Provider>
            <h1>Jotai State Management Example</h1>
            <DisplayComponent />
            <ModifyComponent />
        </Provider>
    );
}

export default App;
```

#### Component to Display State
```jsx
// DisplayComponent.js
import React from 'react';
import { useAtom } from 'jotai';
import { jsonStateAtom } from './atoms';

function DisplayComponent() {
    const [jsonState] = useAtom(jsonStateAtom);

    return (
        <div>
            <h2>Current State:</h2>
            <pre>{JSON.stringify(jsonState, null, 2)}</pre>
        </div>
    );
}

export default DisplayComponent;
```

#### Component to Modify State
```jsx
// ModifyComponent.js
import React from 'react';
import { useAtom } from 'jotai';
import { jsonStateAtom } from './atoms';

function ModifyComponent() {
    const [jsonState, setJsonState] = useAtom(jsonStateAtom);

    const updateKey = () => {
        setJsonState({ ...jsonState, key1: 'updatedValue' });
    };

    return (
        <div>
            <h2>Modify State:</h2>
            <button onClick={updateKey}>Update key1</button>
        </div>
    );
}

export default ModifyComponent;
```

### How It Works
1. `jsonStateAtom` holds the central JSON object.
2. `DisplayComponent` reads and displays the current state.
3. `ModifyComponent` updates the state using `setJsonState`. The changes are immediately reflected in `DisplayComponent`.
