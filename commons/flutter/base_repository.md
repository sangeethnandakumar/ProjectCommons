# Flutter Generic Repo
Generics supported repository structure interface in Flutter

## base_repository.dart
```dart
abstract class BaseRepository<T> {
  Future<void> create(T item);
  Future<T?> getById(String id);
  Future<List<T>> getAll();
  Future<void> update(T item);
  Future<void> delete(String id);
}
```
