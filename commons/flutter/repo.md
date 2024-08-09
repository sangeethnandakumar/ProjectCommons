# REPOSITORY IMPLEMENTATION
Implementation of Generic abstraction/interface

Contains inherited methods as well as private methods

<hr/>

## abstractions.dart
```dart
import '../../models/record_model.dart';
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
    return sortRecordsByDate(data.values.map((json) => RecordModel.fromJson(json)).toList());
  }

  List<RecordModel> sortRecordsByDate(List<RecordModel> records) {
    records.sort((a, b) {
      return b.date.compareTo(a.date);
    });
    return records;
  }

  @override
  Future<List<RecordModel>> getRange(String start, String end) async {
    // Read JSON data from local storage
    final data = await _localStorage.readJson(_filename);

    // Convert the JSON data to a list of RecordModel objects
    List<RecordModel> allRecords = data.values.map((json) => RecordModel.fromJson(json)).toList();

    // Parse the start and end dates from strings to DateTime
    DateTime startDate = DateTime.parse(start);
    DateTime endDate = DateTime.parse(end);

    // Filter records within the specified date range
    List<RecordModel> filteredRecords = allRecords.where((record) {
      return record.date.isAfter(startDate) && record.date.isBefore(endDate);
    }).toList();

    return sortRecordsByDate(filteredRecords);
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

  @override
  Future<void> deleteByCategory(String categoryId) async {
    final data = await _localStorage.readJson(_filename);

    // Filter out records that belong to the specified category
    final filteredData = data.values.where((recordJson) {
      final record = RecordModel.fromJson(recordJson);
      return record.category != categoryId; // Keep records that don't match the categoryId
    }).toList();

    // Create a new map with the filtered records
    final newData = { for (var record in filteredData) (record as RecordModel).id : record };
    await _localStorage.writeJson(_filename, newData);
  }


  // Additional method to fetch records by date range
  Future<List<RecordModel>> getRecordsByDateRange(DateTime start, DateTime end) async {
    final records = await getAll();
    return records.where((record) => record.date.isAfter(start) && record.date.isBefore(end)).toList();
  }

  // Additional method to sort records by expense amount
  Future<List<RecordModel>> getSortedRecords() async {
    final records = await getAll();
    records.sort((a, b) => a.amt.compareTo(b.amt));
    return records;
  }
}
```
