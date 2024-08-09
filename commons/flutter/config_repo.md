# CONFIG REPO
A common style Config repo for all sort of app configurations

<hr/>

## Config Schema
```json
{
	"instance": "defaultInstance",
	"isFirstRun": true,
	"deviceId": "<UNQUE DEVICE ID>",
	"user": {
		"id": "3456",
		"firstName": "Sangeeth",
		"lastName": "Nandakumar",
		"email": "sangeethnandakumar@gmail.com"
	},
	"update": {
		"forceUpdate": false,
		"latest": 1.0,
		"title": "Intial Update",
		"changelog": "First version of the app",
	},
	"rating": {
		"forcePrompt": false,
		"title": "Rate if you enjoy the app. Your rating will be a lot helpfull"
	},
	"subscription": {
		"isSubscribed": false,
		"subscriptionPlan": null
	},
	"devices": [
		{
			"id": "<UNQUE DEVICE ID>",
			"model": "Galaxy S22",
			"brand": "Samsung"
		},
		{
			"id": "<ANOTHER UNQUE DEVICE ID>",
			"model": "Air",
			"brand": "Thinkpad"
		}
	],
	"misc": [
		{"key": "A32", "value": "decoy"},
		{"key": "A44", "value": "seperate"},
		{"key": "A26", "value": "na"}
	]
}
```

## config_repo.dart
```dart
import 'dart:io';
import 'package:device_info_plus/device_info_plus.dart';
import '../../models/config_model.dart';
import '../abstractions.dart';
import '../local_storage.dart';
import 'package:shared_preferences/shared_preferences.dart';

class ConfigRepository extends BaseRepository<ConfigModel> {
  final LocalStorage _localStorage = LocalStorage();
  final String _filename = 'config.json';
  final DeviceInfoPlugin _deviceInfo = DeviceInfoPlugin();

  @override
  Future<void> create(ConfigModel config) async {
    final data = await _localStorage.readJson(_filename);
    data[config.instance ?? 'defaultInstance'] = config.toJson();
    await _localStorage.writeJson(_filename, data);
  }

  @override
  Future<void> update(ConfigModel config) async {
    final data = await _localStorage.readJson(_filename);
    if (data.containsKey(config.instance)) {
      data[config.instance!] = config.toJson();
      await _localStorage.writeJson(_filename, data);
    }
  }

  @override
  Future<List<ConfigModel>> getAll() async {
    final data = await _localStorage.readJson(_filename);
    return data.values.map((json) => ConfigModel.fromJson(json)).toList();
  }

  @override
  Future<ConfigModel?> getById(String instance) async {
    final data = await _localStorage.readJson(_filename);
    var result = data[instance] != null ? ConfigModel.fromJson(data[instance]) : null;

    if (result == null) {
      var deviceInfo = await _getDeviceInfo();
      var newConfig = ConfigModel(
        instance: 'defaultInstance',
        isFirstRun: true,
        deviceId: deviceInfo['deviceId'],
        user: null,
        update: Update(
          forceUpdate: false,
          latest: 1.0,
          title: "Initial Update",
          changelog: "First version of the app",
        ),
        rating: Rating(
          forcePrompt: false,
          title: "Rate if you enjoy the app. Your rating will be a lot helpful",
        ),
        subscription: Subscription(
          isSubscribed: false,
          subscriptionPlan: null,
        ),
        devices: [
          Device(
            id: deviceInfo['deviceId'],
            model: deviceInfo['model'],
            brand: deviceInfo['brand'],
          )
        ],
        misc: [],
      );
      await create(newConfig);
      return newConfig;
    }
    return result;
  }

  @override
  Future<void> delete(String instance) async {
    final data = await _localStorage.readJson(_filename);
    if (data.containsKey(instance)) {
      data.remove(instance);
      await _localStorage.writeJson(_filename, data);
    }
  }

  Future<Map<String, String>> _getDeviceInfo() async {
    String? deviceId = await _getDeviceId();
    String model = '';
    String brand = '';

    if (Platform.isAndroid) {
      AndroidDeviceInfo androidInfo = await _deviceInfo.androidInfo;
      model = androidInfo.model;
      brand = androidInfo.brand;
    } else if (Platform.isIOS) {
      IosDeviceInfo iosInfo = await _deviceInfo.iosInfo;
      model = iosInfo.utsname.machine;
      brand = 'Apple';
    }

    return {
      'deviceId': deviceId ?? 'unknown_device_id',
      'model': model,
      'brand': brand,
    };
  }

  Future<String?> _getDeviceId() async {
    final SharedPreferences prefs = await SharedPreferences.getInstance();
    String? deviceId = prefs.getString('device_id');

    if (deviceId == null) {
      if (Platform.isAndroid) {
        AndroidDeviceInfo androidInfo = await _deviceInfo.androidInfo;
        deviceId = androidInfo.id;
      } else if (Platform.isIOS) {
        IosDeviceInfo iosInfo = await _deviceInfo.iosInfo;
        deviceId = iosInfo.identifierForVendor;
      }
      // Store the device ID for future use
      if (deviceId != null) {
        await prefs.setString('device_id', deviceId);
      }
    }
    return deviceId;
  }
}
```

## Models
```dart
// lib/models/config_model.dart
class ConfigModel {
  final String? instance;
  bool isFirstRun;
  final String? deviceId;
  final User? user;
  final Update? update;
  final Rating? rating;
  final Subscription? subscription;
  final List<Device>? devices;
  final List<Misc>? misc;

  ConfigModel({
    required this.instance,
    required this.isFirstRun,
    required this.deviceId,
    required this.user,
    required this.update,
    required this.rating,
    required this.subscription,
    required this.devices,
    required this.misc,
  });

  factory ConfigModel.fromJson(Map<String, dynamic> json) {
    return ConfigModel(
      instance: json['instance'],
      isFirstRun: json['isFirstRun'] ?? false, // Default to false if null
      deviceId: json['deviceId'],
      user: json['user'] != null ? User.fromJson(json['user']) : null,
      update: json['update'] != null ? Update.fromJson(json['update']) : null,
      rating: json['rating'] != null ? Rating.fromJson(json['rating']) : null,
      subscription: json['subscription'] != null ? Subscription.fromJson(json['subscription']) : null,
      devices: (json['devices'] as List<dynamic>?)
          ?.map((e) => Device.fromJson(e))
          .toList() ?? [], // Default to an empty list if null
      misc: (json['misc'] as List<dynamic>?)
          ?.map((e) => Misc.fromJson(e))
          .toList() ?? [], // Default to an empty list if null
    );
  }


  Map<String, dynamic> toJson() {
    return {
      'instance': instance,
      'isFirstRun': isFirstRun,
      'deviceId': deviceId,
      'user': user?.toJson(),
      'update': update?.toJson(),
      'rating': rating?.toJson(),
      'subscription': subscription?.toJson(),
      'devices': devices?.map((e) => e.toJson()).toList(),
      'misc': misc?.map((e) => e.toJson()).toList(),
    };
  }
}

class User {
  final String id;
  final String firstName;
  final String lastName;
  final String email;

  User({
    required this.id,
    required this.firstName,
    required this.lastName,
    required this.email,
  });

  factory User.fromJson(Map<String, dynamic> json) {
    return User(
      id: json['id'],
      firstName: json['firstName'],
      lastName: json['lastName'],
      email: json['email'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'firstName': firstName,
      'lastName': lastName,
      'email': email,
    };
  }
}

class Update {
  final bool forceUpdate;
  final double latest;
  final String title;
  final String changelog;

  Update({
    required this.forceUpdate,
    required this.latest,
    required this.title,
    required this.changelog,
  });

  factory Update.fromJson(Map<String, dynamic> json) {
    return Update(
      forceUpdate: json['forceUpdate'],
      latest: json['latest'].toDouble(),
      title: json['title'],
      changelog: json['changelog'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'forceUpdate': forceUpdate,
      'latest': latest,
      'title': title,
      'changelog': changelog,
    };
  }
}

class Rating {
  final bool forcePrompt;
  final String title;

  Rating({
    required this.forcePrompt,
    required this.title,
  });

  factory Rating.fromJson(Map<String, dynamic> json) {
    return Rating(
      forcePrompt: json['forcePrompt'],
      title: json['title'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'forcePrompt': forcePrompt,
      'title': title,
    };
  }
}

class Subscription {
  final bool isSubscribed;
  final String? subscriptionPlan;

  Subscription({
    required this.isSubscribed,
    required this.subscriptionPlan,
  });

  factory Subscription.fromJson(Map<String, dynamic> json) {
    return Subscription(
      isSubscribed: json['isSubscribed'],
      subscriptionPlan: json['subscriptionPlan'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'isSubscribed': isSubscribed,
      'subscriptionPlan': subscriptionPlan,
    };
  }
}

class Device {
  final String? id;
  final String? model;
  final String? brand;

  Device({
    required this.id,
    required this.model,
    required this.brand,
  });

  factory Device.fromJson(Map<String, dynamic> json) {
    return Device(
      id: json['id'],
      model: json['model'],
      brand: json['brand']
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'model': model,
      'brand': brand
    };
  }
}

class Misc {
  final String key;
  final String value;

  Misc({
    required this.key,
    required this.value,
  });

  factory Misc.fromJson(Map<String, dynamic> json) {
    return Misc(
      key: json['key'],
      value: json['value'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'key': key,
      'value': value,
    };
  }
}
```
