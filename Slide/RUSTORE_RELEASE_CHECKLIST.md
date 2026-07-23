# RuStore release checklist

## Project state

- Unity: 6000.4.10f1.
- Android package name: `com.strazed.slide`.
- Current app version: `1.1.2`.
- Current Android `versionCode`: `9`.
- Android min SDK: `25`.
- Android target SDK: `34`.
- Android build scripting define: `RUSTORE_BUILD`.
- First RuStore version: no ads and no real-money in-app purchases.
- Unity IAP package is removed from `Packages/manifest.json` for the first RuStore release so the Android build does not include Google BillingClient or its Kotlin/AndroidX dependencies. The old IAP flow in `UnityStore.cs` is kept behind the `USE_UNITY_IAP` scripting define for a later paid version.
- Production keystore: `/Users/timurahmetov/Desktop/SLIDE-Dev/keystore.keystore`.
- Production key alias: `upload`.
- Production certificate SHA-256: `F6:7C:87:0E:1F:4C:60:42:42:D0:07:66:9F:3B:18:AC:2F:ED:61:07:D3:F8:2D:E4:73:90:E0:65:DD:46:74:47`.

## Before upload

- Check that the release build is signed with the production keystore. The project currently points to `/Users/timurahmetov/Desktop/SLIDE-Dev/keystore.keystore`, and the password from `/Users/timurahmetov/Desktop/SLIDE-Dev/pass.txt` was verified for both the keystore and alias `upload`.
- Keep `keystore.keystore` and `pass.txt` private. Losing either the key or the password can block updates for the published app.
- If this is not the first published build for this package, increment `AndroidBundleVersionCode` above the currently published value.
- Build and test a release APK or AAB on a real Android device.
- Verify the app launches, gameplay works, ads/purchases do not crash, and no test/debug UI is visible.
- Remove or replace any links to Google Play or other third-party app stores in the RuStore build.
- The custom Android manifest in `Assets/Plugins/Android/AndroidManifest.xml` should not set `android:icon` or `android:label`; Unity 6 generates those resources in the launcher module, and putting them in the library manifest breaks `verifyReleaseResources`.
- Old Google Mobile Ads/Firebase Android files were moved to `Assets/DisabledLegacyAndroid~`. If ads/analytics are needed later, restore them only after updating the SDKs for Unity 6 and re-checking final manifest permissions.
- The old UDP Android plugin was moved to `Assets/DisabledLegacyAndroid~/UDP` because its Android libraries conflict with Unity 6 Gradle dependencies and caused `checkReleaseDuplicateClasses` Kotlin duplicate-class failures. Do not restore it for the first RuStore release.
- The old Unity 2018 Gradle template was moved to `Assets/Plugins/Android/Unity2018Templates~/mainTemplate.gradle`. Do not re-enable it as-is; regenerate or replace templates for Unity 6 before adding RuStore Pay Gradle dependencies.
- The old External Dependency Manager cache was moved to `ProjectSettings/AndroidResolverDependencies.xml.unity2018-disabled`.

## RuStore console

- Create the app in RuStore Console with package name `com.strazed.slide`.
- Upload APK or AAB. APK must be signed; AAB signatures are configured separately in RuStore.
- Fill in data safety and permission declarations based on the final Android manifest.
- Prepare store listing:
  - app name up to 30 characters;
  - short description up to 80 characters;
  - full description up to 4000 characters;
  - category, age rating, content tags;
  - developer contact email or another required contact;
  - 512x512 PNG/JPG icon with filled background;
  - 1-10 real gameplay screenshots.

## RuStore Pay

The current project uses Unity IAP product IDs:

- `skin10`
- `skin11`
- `skin12`
- `no_ads`
- `all_pack`

RuStore Pay is not needed for the first release because real-money purchases are disabled for `RUSTORE_BUILD`.

For a later version with in-app purchases:

- Re-add `com.unity.purchasing` only if you need the old Google/iOS Unity IAP path, and enable the `USE_UNITY_IAP` scripting define for that build.
- Create matching products in RuStore Console, or decide new product IDs and update the code.
- Import RuStore Pay SDK for Unity.
- In Unity, open `Window -> RuStoreSDK -> Settings -> PayClient`.
- Set `consoleApplicationId` from the RuStore Console app URL.
- Set a unique `deeplinkScheme`.
- Run the SDK manifest patcher.
- Run External Dependency Manager `Force Resolve`.
- Enable custom Gradle templates only after regenerating Unity 6-compatible templates.
- Keep Android entry activity as `UnityPlayerActivity`.
- Replace the Unity IAP purchase flow with a RuStore Pay flow for builds using `RUSTORE_BUILD`.

## Documentation

- Publication: https://www.rustore.ru/help/developers/publishing-and-verifying-apps/app-publication
- App requirements: https://www.rustore.ru/help/developers/publishing-and-verifying-apps/requirement-apps
- APK/AAB signing: https://www.rustore.ru/help/developers/publishing-and-verifying-apps/app-publication/apk-signature
- RuStore Pay Unity SDK: https://www.rustore.ru/help/sdk/pay/unity/10-5-0
