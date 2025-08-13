# Create 2 files
Create these 2 files on root of client project

- .env
- .env.production

## Content format
Always start with `VITE_APP_` for Vite to pick it up

```env
VITE_APP_API_URL=https://localhost:49155
```

## Commit Both The Files to Git

## Usage
Use as follows in HTML or Js files `import.meta.env.VITE_APP_API_URL`
```jsx
 <div className="row">
     <div className="col-12">
         <div className="card">
             <div className="card-body">
                 <img src={`${import.meta.env.VITE_APP_API_URL}/myimage.png`}/>
             </div>
         </div>
     </div>
 </div>
```

## Ensure UTF-8
Ensure proper UTF8 encoding & BOM is not added

<img width="757" height="222" alt="image" src="https://github.com/user-attachments/assets/89646c66-977d-4524-b328-cb1bcd60fe99" />
