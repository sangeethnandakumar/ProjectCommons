# Flutter Init Setup

## D:\myflutter\pubspec.yaml
With RiverPod preinstalled, update version as nessasary

```yml
name: myflutter
description: A new Flutter project.
publish_to: 'none'

version: 1.0.0+1

environment:
  sdk: ^3.5.0

dependencies:
  flutter:
    sdk: flutter
  cupertino_icons: ^1.0.8
  flutter_riverpod: ^2.6.1

dev_dependencies:
  flutter_test:
    sdk: flutter
  flutter_lints: ^4.0.0

flutter:
  uses-material-design: true
```

## main.dart
```dart
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

//Global States
final counterProvider = StateProvider((ref) => 1);

//Provider Wrapping
void main() => runApp(const ProviderScope(child: MyApp()));

//Consumer Widget
class MyApp extends ConsumerWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) => MaterialApp(
    home: Scaffold(
      appBar: AppBar(title: const Text('Counter App')),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [

            //Read State
            Text('Count: ${ref.watch(counterProvider)}', style: Theme.of(context).textTheme.headlineMedium),

            ElevatedButton(
              //Write State
              onPressed: () => ref.read(counterProvider.notifier).state++,
              child: const Text('Increment Counter'),
            )
          ],
        ),
      ),
    ),
  );
}
```

## D:\myflutter\android\settings.gradle

```gradle
pluginManagement {
    def flutterSdkPath = {
        def properties = new Properties()
        file("local.properties").withInputStream { properties.load(it) }
        def flutterSdkPath = properties.getProperty("flutter.sdk")
        assert flutterSdkPath != null, "flutter.sdk not set in local.properties"
        return flutterSdkPath
    }()

    includeBuild("$flutterSdkPath/packages/flutter_tools/gradle")

    repositories {
        google()
        mavenCentral()
        gradlePluginPortal()
    }
}

plugins {
    id "dev.flutter.flutter-plugin-loader" version "1.0.0"
    id "com.android.application" version "8.1.2" apply false
    id "org.jetbrains.kotlin.android" version "2.0.10" apply false
}

include ":app"
```

## D:\myflutter\android\gradle\wrapper\gradle-wrapper.properties

> Goto `https://services.gradle.org/distributions/` to get the latest version and put in `distributionUrl` below.
> Then update version of `id "com.android.application" version "8.1.2" apply false` in above Gradle Plugin section

```properties
distributionBase=GRADLE_USER_HOME
distributionPath=wrapper/dists
zipStoreBase=GRADLE_USER_HOME
zipStorePath=wrapper/dists
distributionUrl=https://services.gradle.org/distributions/gradle-8.12-all.zip
```
