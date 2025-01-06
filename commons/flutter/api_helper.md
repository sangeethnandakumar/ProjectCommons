# api_helper.dart
Centralised single point for all API calls works like Api.js

```dart
import 'dart:convert';
import 'package:http/http.dart' as http;

class ApiHelper {
  final String baseUrl;

  ApiHelper(this.baseUrl);

  Future<void> _makeRequest({
    required String method,
    required String endpoint,
    Map<String, String>? headers,
    dynamic body,
    Map<String, String>? queryParams,
    Function(dynamic)? onSuccess,
    Function(dynamic)? onError,
  }) async {
    try {
      Uri url = Uri.parse('$baseUrl$endpoint').replace(queryParameters: queryParams);

      headers ??= {'Content-Type': 'application/json'};
      var response;

      switch (method) {
        case 'GET':
          response = await http.get(url, headers: headers);
          break;
        case 'POST':
          response = await http.post(url, headers: headers, body: jsonEncode(body));
          break;
        case 'PUT':
          response = await http.put(url, headers: headers, body: jsonEncode(body));
          break;
        case 'PATCH':
          response = await http.patch(url, headers: headers, body: jsonEncode(body));
          break;
        case 'DELETE':
          response = await http.delete(url, headers: headers);
          break;
        default:
          throw Exception('Unsupported method: $method');
      }

      if (response.statusCode >= 200 && response.statusCode < 300) {
        if (onSuccess != null) onSuccess(jsonDecode(response.body));
      } else {
        if (onError != null) onError(jsonDecode(response.body));
      }
    } catch (e) {
      if (onError != null) onError({'error': e.toString()});
    }
  }

  void get(
    String endpoint, {
    Map<String, String>? headers,
    Map<String, String>? queryParams,
    Function(dynamic)? onSuccess,
    Function(dynamic)? onError,
  }) {
    _makeRequest(
      method: 'GET',
      endpoint: endpoint,
      headers: headers,
      queryParams: queryParams,
      onSuccess: onSuccess,
      onError: onError,
    );
  }

  void post(
    String endpoint, {
    Map<String, String>? headers,
    dynamic body,
    Function(dynamic)? onSuccess,
    Function(dynamic)? onError,
  }) {
    _makeRequest(
      method: 'POST',
      endpoint: endpoint,
      headers: headers,
      body: body,
      onSuccess: onSuccess,
      onError: onError,
    );
  }

  void put(
    String endpoint, {
    Map<String, String>? headers,
    dynamic body,
    Function(dynamic)? onSuccess,
    Function(dynamic)? onError,
  }) {
    _makeRequest(
      method: 'PUT',
      endpoint: endpoint,
      headers: headers,
      body: body,
      onSuccess: onSuccess,
      onError: onError,
    );
  }

  void patch(
    String endpoint, {
    Map<String, String>? headers,
    dynamic body,
    Function(dynamic)? onSuccess,
    Function(dynamic)? onError,
  }) {
    _makeRequest(
      method: 'PATCH',
      endpoint: endpoint,
      headers: headers,
      body: body,
      onSuccess: onSuccess,
      onError: onError,
    );
  }

  void delete(
    String endpoint, {
    Map<String, String>? headers,
    Function(dynamic)? onSuccess,
    Function(dynamic)? onError,
  }) {
    _makeRequest(
      method: 'DELETE',
      endpoint: endpoint,
      headers: headers,
      onSuccess: onSuccess,
      onError: onError,
    );
  }
}
```

## Example Usage

```dart
void main() {
  final api = ApiHelper('https://jsonplaceholder.typicode.com');

  // Example: GET request
  api.get(
    '/posts/1',
    onSuccess: (data) {
      print('Success: $data');
    },
    onError: (error) {
      print('Error: $error');
    },
  );

  // Example: POST request
  api.post(
    '/posts',
    body: {'title': 'foo', 'body': 'bar', 'userId': 1},
    onSuccess: (data) {
      print('Created: $data');
    },
    onError: (error) {
      print('Error: $error');
    },
  );
}
```
