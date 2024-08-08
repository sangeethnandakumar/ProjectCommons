# Allows Writing Data Into Flutter Local Persistance

> Required package `path_provider: ^2.1.4`

<hr/>

# A. Local Storage
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

# B. Abstractions
```dart
import '../models/CategoryModel.dart';

// Generic Base Repo
abstract class BaseRepository<T> {
  Future<void> create(T item);
  Future<T?> getById(String id);
  Future<List<T>> getAll();
  Future<void> update(T item);
  Future<void> delete(String id);
}

// Sub Repos
abstract class RecordRepository {
  Future<void> createRecord(Record record);
  Future<Record?> getRecordById(String id);
  Future<List<Record>> getAllRecords();
  Future<void> updateRecord(Record record);
  Future<void> deleteRecord(String id);
}

abstract class CategoryRepository {
  Future<void> createCategory(CategoryModel category);
  Future<CategoryModel?> getCategoryById(String id);
  Future<List<CategoryModel>> getAllCategories();
  Future<void> updateCategory(CategoryModel category);
  Future<void> deleteCategory(String id);
}
```

# C. Implementations
```dart
import '../../models/Record.dart';
import '../abstractions.dart';
import '../local_storage.dart';

class RecordRepository extends BaseRepository<RecordModel> {
  final LocalStorage _localStorage = LocalStorage();
  final String _filename = 'records.json';

  @override
  Future<void> create(RecordModel record) async {
    final data = await _localStorage.readJson(_filename);
    data[record.id] = record; // Use record ID as the key
    await _localStorage.writeJson(_filename, data);
  }

  @override
  Future<RecordModel?> getById(String id) async {
    final data = await _localStorage.readJson(_filename);
    return data.containsKey(id) ? RecordModel.fromJson(data[id]) : null;
  }

  @override
  Future<List<RecordModel>> getAll() async {
    final data = await _localStorage.readJson(_filename);
    return data.values.map((json) => RecordModel.fromJson(json)).toList();
  }

  @override
  Future<void> update(RecordModel record) async {
    final data = await _localStorage.readJson(_filename);
    if (data.containsKey(record.id)) {
      data[record.id] = record;
      await _localStorage.writeJson(_filename, data);
    }
  }

  @override
  Future<void> delete(String id) async {
    final data = await _localStorage.readJson(_filename);
    data.remove(id);
    await _localStorage.writeJson(_filename, data);
  }

  // Additional method to fetch records by date range
  Future<List<RecordModel>> getRecordsByDateRange(DateTime start, DateTime end) async {
    final records = await getAll();
    return records.where((record) => record.date.isAfter(start) && record.date.isBefore(end)).toList();
  }

  // Additional method to sort records by expense amount
  Future<List<RecordModel>> getSortedRecords() async {
    final records = await getAll();
    records.sort((a, b) => a.exp.compareTo(b.exp));
    return records;
  }
}
```
