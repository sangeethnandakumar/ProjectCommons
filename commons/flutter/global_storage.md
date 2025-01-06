# global_storage.md

Generic local Key-Value storage for Flutter apps

```dart
import 'package:path/path.dart';
import 'dart:convert';

class GlobalStorage {
  static const String _storageFile = 'global.json';
  final LocalStorage _localStorage = LocalStorage();

  Future<T?> get<T>(String key) async {
    final data = await _localStorage.readJson(_storageFile);
    final value = data[key];
    
    if (value == null) return null;
    
    switch (T) {
      case String:
        return value.toString() as T;
      case int:
        return int.tryParse(value.toString()) as T?;
      case double:
        return double.tryParse(value.toString()) as T?;
      case bool:
        return (value.toString().toLowerCase() == 'true') as T;
      default:
        throw UnsupportedError('Type ${T.toString()} is not supported');
    }
  }

  Future<void> set<T>(String key, T value) async {
    final data = await _localStorage.readJson(_storageFile);
    data[key] = value.toString();
    await _localStorage.writeJson(_storageFile, data);
  }

  Future<void> remove(String key) async {
    final data = await _localStorage.readJson(_storageFile);
    data.remove(key);
    await _localStorage.writeJson(_storageFile, data);
  }

  Future<bool> hasKey(String key) async {
    final data = await _localStorage.readJson(_storageFile);
    return data.containsKey(key);
  }

  Future<void> clear() async {
    await _localStorage.writeJson(_storageFile, {});
  }
}
```

## Example Usage

```
final storage = GlobalStorage();

//Get
final username = await storage.get<String>('username');
final age = await storage.get<int>('age');

//Set
await storage.set('username', 'john_doe');
```
