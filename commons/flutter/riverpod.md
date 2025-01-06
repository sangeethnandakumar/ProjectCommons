
# Riverpod Setup for `StateProvider` and `FutureProvider`

## 1. `StateProvider` Setup

### Step 1: Add Riverpod dependency to `pubspec.yaml`
```
dependencies:
  flutter_riverpod: ^2.0.0
```

### Step 2: Create a `StateProvider`
```
final counterProvider = StateProvider<int>((ref) => 0);
```

### Step 3: Create a `ConsumerWidget` to read state
```
class CounterPage extends ConsumerWidget {
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final count = ref.watch(counterProvider);
    return ElevatedButton(
      onPressed: () => ref.read(counterProvider.notifier).state++,
      child: Text('Count: $count'),
    );
  }
}
```

## 2. `FutureProvider` Setup

### Step 1: Add Riverpod dependency to `pubspec.yaml`
```
dependencies:
  flutter_riverpod: ^2.0.0
```

### Step 2: Create a `FutureProvider`
```
final userProvider = FutureProvider<List<User>>((ref) async {
  return await fetchUsersFromApi();
});
```

### Step 3: Create a `ConsumerWidget` to watch the provider
```
class UserListPage extends ConsumerWidget {
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final asyncUsers = ref.watch(userProvider);
    return asyncUsers.when(
      data: (users) => ListView.builder(
        itemCount: users.length,
        itemBuilder: (context, index) => ListTile(title: Text(users[index].name)),
      ),
      loading: () => CircularProgressIndicator(),
      error: (error, stack) => Text('Error: $error'),
    );
  }
}
```

