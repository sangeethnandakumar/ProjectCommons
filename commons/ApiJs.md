# Api.js

```js
import axios from 'axios';

const baseURL = 'http://localhost:5093';
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
    if (!response || !callbacks) {
        window.location.href = "/login";
    }
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

const Api = {
    GET: async (url, params, callbacks, msalInstance = null) => await makeRequest('GET', url, params, callbacks, msalInstance),
    POST: async (url, data, callbacks, msalInstance = null) => await makeRequest('POST', url, data, callbacks, msalInstance),
    PUT: async (url, data, callbacks, msalInstance = null) => await makeRequest('PUT', url, data, callbacks, msalInstance),
    PATCH: async (url, data, callbacks, msalInstance = null) => await makeRequest('PATCH', url, data, callbacks, msalInstance),
    DELETE: async (url, callbacks, msalInstance = null) => await makeRequest('DELETE', url, null, callbacks, msalInstance),
};

export default Api;
```

### Usage

- Install Axios (`npm i axios`)
- Replace `baseURL` below
- Replace `scopes` below
- Ready to use

> Based on `bool` value we pass in `isFormData`, Api.js will either put data as Key-Value FormData or as Raw JSON body

> If you didn't pass `msalInstance`, Api.js assumes authorization is not required for that endpoint and never try token accusation

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

// Example PUT request
const putData = { id: 123, name: 'Updated Name' };
Api.PUT(
  '/api/update', 
  putData, 
  {
    onSuccess: (data) => {
      console.log('PUT request successful:', data);
    },
    onError: (error) => {
      console.error('Error in PUT request:', error);
    },
    onBadRequest: (data) => {
      console.error('Bad request in PUT:', data);
    },
    onUnauthorized: (data) => {
      console.error('Unauthorized in PUT:', data);
    },
    onForbid: (data) => {
      console.error('Forbidden in PUT:', data);
    }
  },
  msalInstance // Optional
);

// Example PATCH request
const patchData = { id: 456, status: 'completed' };
Api.PATCH(
  '/api/modify', 
  patchData, 
  {
    onSuccess: (data) => {
      console.log('PATCH request successful:', data);
    },
    onError: (error) => {
      console.error('Error in PATCH request:', error);
    },
    onBadRequest: (data) => {
      console.error('Bad request in PATCH:', data);
    },
    onUnauthorized: (data) => {
      console.error('Unauthorized in PATCH:', data);
    },
    onForbid: (data) => {
      console.error('Forbidden in PATCH:', data);
    }
  },
  msalInstance // Optional
);

// Example DELETE request
Api.DELETE(
  '/api/delete/123', 
  {
    onSuccess: (data) => {
      console.log('DELETE request successful:', data);
    },
    onError: (error) => {
      console.error('Error in DELETE request:', error);
    },
    onBadRequest: (data) => {
      console.error('Bad request in DELETE:', data);
    },
    onUnauthorized: (data) => {
      console.error('Unauthorized in DELETE:', data);
    },
    onForbid: (data) => {
      console.error('Forbidden in DELETE:', data);
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
