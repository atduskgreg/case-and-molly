using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using WebSocketSharp;

namespace WebSocketSharp.Unity.Editor
{
	public class MenuExtension : MonoBehaviour
	{
		const string _version = "1.0.4";

		[MenuItem ("websocket-sharp/Echo Back Test")]
		static void EchoBack ()
		{
			string res = null;
			using (var ws = new WebSocket ("ws://echo.websocket.org"))
			//using (var ws = new WebSocket ("ws://localhost:4649/Echo"))
			{
				var ver = Application.unityVersion;
				ws.OnOpen += (sender, e) =>
				{
					ws.Send (String.Format ("Hello, Unity {0}!", ver));
				};

				ws.OnMessage += (sender, e) =>
				{
					res = e.Data;
				};

				ws.OnError += (sender, e) =>
				{
					Debug.LogError (e.Message);
				};

				ws.Connect ();
			}

			if (!res.IsNullOrEmpty())
				EditorUtility.DisplayDialog ("Echo Back Successfully!", res, "OK");
		}

		[MenuItem ("websocket-sharp/Start Test Server")]
		static void StartServer ()
		{
			EditorWindow.GetWindow<ServerMonitor> (true, "Server Monitor");
		}

		[MenuItem ("websocket-sharp/About websocket-sharp")]
		static void ShowInfo ()
		{
			var asm = Assembly.GetAssembly (typeof (WebSocket));
			var ver = asm.GetName ().Version.ToString ();
			var description = "Provides the WebSocket protocol client and server.";
			EditorUtility.DisplayDialog ("websocket-sharp for Unity",
				String.Format (
@"{0}

Version {1}

Current websocket-sharp.dll Version {2}

Current Unity Version {3}

Current Mono Runtime Version {4}",
					description,
					_version,
					ver,
					Application.unityVersion,
					Environment.Version.ToString ()),
				"OK");
		}
	}
}
