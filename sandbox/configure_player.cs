UnityEditor.PlayerSettings.companyName = "Jordan World";
UnityEditor.PlayerSettings.productName = "Sandbox";
UnityEditor.PlayerSettings.bundleVersion = "0.1.0";

var android = UnityEditor.Build.NamedBuildTarget.Android;
UnityEditor.PlayerSettings.SetApplicationIdentifier(android, "app.jordanworld.sandbox");
UnityEditor.PlayerSettings.SetScriptingBackend(android, UnityEditor.ScriptingImplementation.IL2CPP);
UnityEditor.PlayerSettings.SetApiCompatibilityLevel(android, UnityEditor.ApiCompatibilityLevel.NET_Standard);

UnityEditor.PlayerSettings.Android.targetArchitectures = UnityEditor.AndroidArchitecture.ARM64;
UnityEditor.PlayerSettings.Android.minSdkVersion = UnityEditor.AndroidSdkVersions.AndroidApiLevel24;
UnityEditor.PlayerSettings.Android.targetSdkVersion = UnityEditor.AndroidSdkVersions.AndroidApiLevelAuto;
UnityEditor.PlayerSettings.Android.bundleVersionCode = 1;

UnityEditor.AssetDatabase.SaveAssets();
return "id=" + UnityEditor.PlayerSettings.GetApplicationIdentifier(android)
    + " backend=" + UnityEditor.PlayerSettings.GetScriptingBackend(android)
    + " arch=" + UnityEditor.PlayerSettings.Android.targetArchitectures
    + " minSdk=" + UnityEditor.PlayerSettings.Android.minSdkVersion;
