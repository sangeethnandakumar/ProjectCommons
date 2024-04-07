# Api.js
Use APi.js for immediate implementation of REST API support
```js
import axios from 'axios';

const baseURL = import.meta.env.VITE_APP_API_URL;
const scopes = [];

const axiosInstance = axios.create({
    baseURL: baseURL,
});

const tryRelogin = msalInstance => {
    if (!msalInstance) return;
    msalInstance.logoutPopup();
    setTimeout(() => msalInstance.loginPopup(), 2000);
}

const fetchMSALToken = async msalInstance => {
    if (!msalInstance) return null;
    try {
        const token = await msalInstance.acquireTokenSilent({
            scopes: scopes,
        });
        return token ? token.accessToken : null;
    } catch (error) {
        if (msalInstance) {
            tryRelogin(msalInstance);
        }
        throw error;
    }
}

const fetchLocalToken = () => window.localStorage.getItem('token');

const handleResponse = (response, callbacks) => {
    const { onSuccess, onError, onBadRequest, onForbid, onUnauthorized } = callbacks;

    if (response.status === 200 && onSuccess) {
        onSuccess(response.data);
    } else if (response.status === 400 && onBadRequest) {
        onBadRequest(response.data);
    } else if (response.status === 401) {
        onUnauthorized && onUnauthorized(response.data);
    } else if (response.status === 403 && onForbid) {
        onForbid(response.data);
    } else if (onError) {
        onError(response.data);
    }
}

const axiosRequest = async (method, url, headers, data = null, params = null) => {
    try {
        let response;
        switch (method) {
            case 'GET':
                response = await axiosInstance.get(url, { headers, params });
                break;
            case 'POST':
                response = await axiosInstance.post(url, data, { headers });
                break;
            case 'PUT':
                response = await axiosInstance.put(url, data, { headers });
                break;
            case 'PATCH':
                response = await axiosInstance.patch(url, data, { headers });
                break;
            case 'DELETE':
                response = await axiosInstance.delete(url, { headers, params });
                break;
            default:
                throw new Error(`Unsupported method: ${method}`);
        }
        return response;
    } catch (error) {
        console.error(error);
        return error.response;
    }
}

const makeRequest = async (method, url, data, callbacks, msalInstance = null) => {
    try {
        const token = msalInstance ? await fetchMSALToken(msalInstance) : fetchLocalToken();
        const headers = {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        };

        const response = await axiosRequest(method, url, headers, data);

        handleResponse(response, callbacks);
    } catch (error) {
        console.warn(error);
        handleResponse(error.response, callbacks);
    }
}

const uploadFile = async (url, file, callbacks, msalInstance = null) => {
    try {
        const token = msalInstance ? await fetchMSALToken(msalInstance) : fetchLocalToken();
        const headers = {
            'Authorization': `Bearer ${token}`,
        };

        const formData = new FormData();
        formData.append('files', file);

        const config = {
            headers: { ...headers, 'Content-Type': 'multipart/form-data' },
            onUploadProgress: (progressEvent) => {
                const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
                callbacks.onUpload && callbacks.onUpload(percentCompleted);
            },
        };

        const response = await axiosInstance.post(url, formData, config);

        if (response.status === 200) {
            callbacks.onSuccessfulUpload && callbacks.onSuccessfulUpload(response.data);
        } else {
            callbacks.onUploadFailure && callbacks.onUploadFailure(response.data);
        }
    } catch (error) {
        console.warn(error);
        callbacks.onUploadFailure && callbacks.onUploadFailure(error.response.data);
    }
};

const Api = {
    GET: async (url, params, callbacks, msalInstance = null) => await makeRequest('GET', url, params, callbacks, msalInstance),
    POST: async (url, data, callbacks, msalInstance = null) => await makeRequest('POST', url, data, callbacks, msalInstance),
    PUT: async (url, data, callbacks, msalInstance = null) => await makeRequest('PUT', url, data, callbacks, msalInstance),
    PATCH: async (url, data, callbacks, msalInstance = null) => await makeRequest('PATCH', url, data, callbacks, msalInstance),
    DELETE: async (url, callbacks, msalInstance = null) => await makeRequest('DELETE', url, null, callbacks, msalInstance),
    UPLOAD: async (url, file, callbacks, msalInstance = null) => await uploadFile(url, file, callbacks, msalInstance),
};

export default Api;
```

### For Upload example see 'UploadDownload.md' file

### Example API calls
```js
import Api from '../libs/api';
import { useMsal } from '@azure/msal-react';

//Get MSAL if using
const { instance } = useMsal();

const App = () => {

// Example GET request
Api.GET(
  '/api/data', 
  null, 
  {
    onSuccess: (data) => {
      console.log('GET request successful:', data);
    },
    onError: (error) => {
      console.error('Error in GET request:', error);
    },
    onBadRequest: (data) => {
      console.error('Bad request in GET:', data);
    },
    onUnauthorized: (data) => {
      console.error('Unauthorized in GET:', data);
    },
    onForbid: (data) => {
      console.error('Forbidden in GET:', data);
    }
  },
  msalInstance // Optional
);

// Example POST request
const postData = { username: 'example', password: 'password123' };
Api.POST(
  '/api/create', 
  postData, 
  {
    onSuccess: (data) => {
      console.log('POST request successful:', data);
    },
    onError: (error) => {
      console.error('Error in POST request:', error);
    },
    onBadRequest: (data) => {
      console.error('Bad request in POST:', data);
    },
    onUnauthorized: (data) => {
      console.error('Unauthorized in POST:', data);
    },
    onForbid: (data) => {
      console.error('Forbidden in POST:', data);
    }
  },
  msalInstance // Optional
);

    return (
        <>
            <h1>Hello</h1>
        </>
    );
}

export default App;
```
