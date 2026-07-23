using System.Collections.Generic;
using UnityEngine;

// namespace Plugins
// {
//     public class FirebaseController
//     {
//
//         public FirebaseController()
//         {
//             Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
//                 var dependencyStatus = task.Result;
//                 if (dependencyStatus == Firebase.DependencyStatus.Available)
//                 {
//                     // Create and hold a reference to your FirebaseApp,
//                     // where app is a Firebase.FirebaseApp property of your application class.
//                     //   app = Firebase.FirebaseApp.DefaultInstance;
//
//                     // Set a flag here to indicate whether Firebase is ready to use by your app.
//                 } else {
//                     UnityEngine.Debug.LogError(System.String.Format(
//                         "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
//                     // Firebase Unity SDK is not safe to use here.
//                 }
//             });
//         }
//
//         public void SimpleLog(string eventId)
//         {
//             Firebase.Analytics.FirebaseAnalytics.LogEvent(eventId);
//         }
//         
//         public void SimpleStringLog(string categoryId, string eventId, string id)
//         {
//             Firebase.Analytics.FirebaseAnalytics.LogEvent(categoryId, eventId, id);
//         }
//         
//         public void SimpleIntLog(string categoryId, string eventId, int time)
//         {
//             Firebase.Analytics.FirebaseAnalytics.LogEvent(categoryId, eventId, time);
//         }
//         
//         public void LogWithParameters(string eventId, Firebase.Analytics.Parameter[] parameters)
//         {
//             Firebase.Analytics.FirebaseAnalytics.LogEvent(
//                 Firebase.Analytics.FirebaseAnalytics.EventLevelUp,
//                 parameters);
//         }
//         
//     }
// }
