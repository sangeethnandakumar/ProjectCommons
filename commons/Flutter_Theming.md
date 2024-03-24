
## app_color.dart

```dart
import 'package:flutter/material.dart';

class AppColor {
  // Text Colors
  static const Color textColor = Colors.black;
  static const Color textColorDark = Colors.white;

  // Background Colors
  static const Color bodyColor = Colors.white;
  static const Color bodyColorDark = Colors.black;

  // Primary Colors (Consider using a base color for variations)
  static const Color primaryColor = Color(0xFF007BFF); // Adjust as needed
  static const Color primaryLight = Color(0xFF00CCFF);
  static const Color primaryDark = Color(0xFF004C9F);

  // Secondary Colors
  static const Color secondaryColor = Color(0xFFFF9800); // Adjust as needed
  static const Color secondaryLight = Color(0xFFFFD369);
  static const Color secondaryDark =  Color(0xFFC76400);

  // Tertiary Color (Optional)
  static const Color tertiaryColor = Color(0xFF4CAF50);
}
```

## light_theme.dart

```dart
import 'package:flutter/material.dart';
import 'app_color.dart';

ThemeData lightTheme = ThemeData(
  brightness: Brightness.light,
  canvasColor: AppColor.bodyColor,
  scaffoldBackgroundColor: AppColor.bodyColor,
  hintColor: AppColor.textColor,
  // Use primaryColor for main buttons
  primaryColor: AppColor.primaryColor,
  colorScheme: ColorScheme.fromSeed(
    seedColor: AppColor.primaryColor,
    primary: AppColor.primaryColor,
    secondary: AppColor.secondaryColor,
    tertiary: AppColor.tertiaryColor,
  ),
  textTheme: const TextTheme(
    headlineLarge: TextStyle(
      color: Colors.black,
      fontSize: 40,
      fontWeight: FontWeight.bold,
    ),
  ),
  // Use individual button themes for better styling and customization
  elevatedButtonTheme: ElevatedButtonThemeData(
    style: ElevatedButton.styleFrom(
      textStyle: const TextStyle(color: Colors.white), // Adjust as needed
      backgroundColor: AppColor.primaryColor,
    ),
  ),
  textButtonTheme: TextButtonThemeData(
    style: TextButton.styleFrom(
      textStyle: const TextStyle(color: AppColor.primaryColor), // Adjust as needed
    ),
  ),
  outlinedButtonTheme: OutlinedButtonThemeData(
    style: OutlinedButton.styleFrom(
      textStyle: const TextStyle(color: AppColor.primaryColor),
      side: const BorderSide(color: AppColor.primaryColor),
    ),
  ),
);
```

## dark_theme.dart

```dart
import 'package:flutter/material.dart';
import 'app_color.dart';

ThemeData darkTheme = ThemeData(
  brightness: Brightness.dark,
  canvasColor: AppColor.bodyColorDark,
  scaffoldBackgroundColor: AppColor.bodyColorDark,
  hintColor: AppColor.textColorDark,
  primaryColor: AppColor.primaryDark,
  colorScheme: ColorScheme.fromSeed(
    seedColor: AppColor.primaryDark,
    primary: AppColor.primaryDark,
    secondary: AppColor.secondaryDark,
    tertiary: AppColor.tertiaryColor,
    brightness: Brightness.dark, // Add this line
  ),
  textTheme: const TextTheme(
    headlineLarge: TextStyle(
      color: Colors.white
    ),
  ),
  elevatedButtonTheme: ElevatedButtonThemeData(
    style: ElevatedButton.styleFrom(
      textStyle: const TextStyle(color: Colors.white),
      backgroundColor: AppColor.primaryDark,
    ),
  ),
  textButtonTheme: TextButtonThemeData(
    style: TextButton.styleFrom(
      textStyle: const TextStyle(color: AppColor.primaryDark),
    ),
  ),
  outlinedButtonTheme: OutlinedButtonThemeData(
    style: OutlinedButton.styleFrom(
      textStyle: const TextStyle(color: AppColor.primaryDark),
      side: const BorderSide(color: AppColor.primaryDark),
    ),
  ),
);
```

## app_theme.dart

```dart
import 'package:flutter/material.dart';
import 'light_theme.dart';
import 'dark_theme.dart';

class AppTheme{
  static ThemeData light = lightTheme;
  static ThemeData dark = darkTheme;
}
```

## default_style.dart

```dart
import 'package:flutter/material.dart';
import 'app_color.dart';

// Heading Styles
const TextStyle headingStyle = TextStyle(
  fontWeight: FontWeight.bold,
  fontSize: 32.0
);

const TextStyle subHeadingStyle = TextStyle(
  fontWeight: FontWeight.w600,
  fontSize: 20.0
);

const TextStyle smallTextStyle = TextStyle(
    fontWeight: FontWeight.w300,
    fontSize: 12.0
);

// Text Styles
const TextStyle bodyTextStyle = TextStyle(
  fontSize: 16.0
);

const TextStyle subtitleTextStyle = TextStyle(
  fontSize: 14.0
);

// Button Styles
final ButtonStyle elevatedButtonStyle = ElevatedButton.styleFrom(
  textStyle: const TextStyle(
    fontSize: 16.0,
    fontWeight: FontWeight.bold,
  ),
  backgroundColor: AppColor.primaryColor,
  foregroundColor: Colors.white,
  padding: const EdgeInsets.symmetric(vertical: 16.0, horizontal: 24.0),
  shape: RoundedRectangleBorder(
    borderRadius: BorderRadius.circular(8.0),
  ),
);

final ButtonStyle outlinedButtonStyle = OutlinedButton.styleFrom(
  textStyle: const TextStyle(
    fontSize: 16.0,
    fontWeight: FontWeight.bold,
  ),
  foregroundColor: AppColor.primaryColor,
  side: BorderSide(color: AppColor.primaryColor),
  padding: const EdgeInsets.symmetric(vertical: 16.0, horizontal: 24.0),
  shape: RoundedRectangleBorder(
    borderRadius: BorderRadius.circular(8.0),
  ),
);

final ButtonStyle textButtonStyle = TextButton.styleFrom(
  textStyle: const TextStyle(
    fontSize: 16.0,
    fontWeight: FontWeight.bold,
  ),
  foregroundColor: AppColor.primaryColor,
  padding: const EdgeInsets.symmetric(vertical: 8.0, horizontal: 16.0),
);
```
