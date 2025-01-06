# Riverpod Setup Guide

## Provider Registration
```dart
// main.dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

void main() => runApp(ProviderScope(child: MyApp()));
```

File structure:
```
lib/
  ├── main.dart
  ├── providers/
  │   ├── counter_provider.dart
  │   └── user_provider.dart
  └── pages/
      ├── counter_page.dart
      └── user_list_page.dart
```

Dependencies:
```yaml
dependencies:
  flutter_riverpod: ^2.4.9
```

<hr/>

## StateProvider Example
```dart
// counter_provider.dart
import 'package:flutter_riverpod/flutter_riverpod.dart';

final counterProvider = StateProvider<int>((ref) => 0); // Track counter state
```

```dart
// counter_page.dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'counter_provider.dart';

class CounterPage extends ConsumerWidget {
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final count = ref.watch(counterProvider);
    return Scaffold(
      body: Column(children: [
        Text('$count'),
        ElevatedButton(
          onPressed: () => ref.read(counterProvider.notifier).state++,
          child: Text('Increment'),
        ),
      ]),
    );
  }
}
```

<hr/>

## FutureProvider Example
```dart
// user_provider.dart
import 'package:flutter_riverpod/flutter_riverpod.dart';

class User {
  final String name;
  User(this.name);
}

Future<List<User>> fetchUsers() async {
  await Future.delayed(Duration(seconds: 2));
  return [User('Alice'), User('Bob')];
}

final userProvider = FutureProvider<List<User>>((ref) => fetchUsers());
```

```dart
// user_list_page.dart 
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'user_provider.dart';

class UserListPage extends ConsumerWidget {
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return ref.watch(userProvider).when(
      data: (users) => ListView.builder(
        itemCount: users.length,
        itemBuilder: (_, i) => Text(users[i].name),
      ),
      loading: () => CircularProgressIndicator(),
      error: (err, _) => Text(err.toString()),
    );
  }
}
```
