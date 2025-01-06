# LOCAL STORAGE BASE

Generic local JSON Object storage for Flutter apps

## local_storage.dart
```dart
import 'dart:convert';
import 'dart:io';
import 'package:path_provider/path_provider.dart';
import 'package:path/path.dart';

class LocalStorage {
  Future<File> _getLocalFile(String filename) async {
    final directory = await getApplicationDocumentsDirectory();
    return File(join(directory.path, filename));
  }

  Future<Map<String, dynamic>> readJson(String filename) async {
    try {
      final file = await _getLocalFile(filename);
      String contents = await file.readAsString();
      return json.decode(contents);
    } catch (e) {
      return {};
    }
  }

  Future<void> writeJson(String filename, Map<String, dynamic> data) async {
    final file = await _getLocalFile(filename);
    await file.writeAsString(json.encode(data));
  }
}
```
