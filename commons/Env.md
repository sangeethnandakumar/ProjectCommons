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
                 <ReactEpubViewer
                         url={`${import.meta.env.VITE_APP_API_URL}/preview/${session}.epub`}
                     onPageChange={onPageChange}
                     ref={viewerRef}
                 />
             </div>
         </div>
     </div>
 </div>
```
