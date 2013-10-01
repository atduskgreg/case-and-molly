using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using WebSocketSharp.Server;

namespace WebSocketSharp.Unity.Editor
{
	public class ServerMonitor : EditorWindow
	{
		StringBuilder   _log;
		GUIStyle        _logStyle;
		Vector2         _scrollPosition;
		WebSocketServer _server;
		object          _sync;
		volatile bool   _updated;

		ServerMonitor ()
		{
			maxSize = new Vector2 (360, 380);
			minSize = maxSize;

			_log = new StringBuilder (64);
			_logStyle = new GUIStyle ();
			_logStyle.fontStyle = FontStyle.Normal;
			//_logStyle.normal.background = ;
			_sync = new object ();
			_updated = false;

			_server = new WebSocketServer (4649);
			_server.Log.Level = LogLevel.TRACE;
			_server.Log.SetOutput (OutputLog);
			_server.OnError += (sender, e) =>
			{
				Debug.LogError (e.Message);
			};

			_server.AddWebSocketService<Echo> ("/Echo");
			_server.AddWebSocketService<Chat> ("/Chat");
			_server.Start ();
		}

		string Log {
			get {
				lock (_sync)
				{
					return _log.ToString ();
				}
			}
		}

		string ServerInfo
		{
			get {
				var info =
@"Listening on
- port: {0}
- service path:
{1}
Could you access to 'ws://localhost:{0} + a service path'?
";
				var spath = new StringBuilder (64);
				foreach (var path in _server.ServicePaths)
					spath.AppendFormat ("  {0}\n", path);

				return String.Format (info, _server.Port, spath.ToString ());
			}
		}

		void OnDestroy ()
		{
			if (_server != null)
				_server.Stop ();
		}

		void OnGUI ()
		{
			GUILayout.Label ("WebSocket Server has been started!", EditorStyles.boldLabel);
			GUILayout.Label (ServerInfo);
			GUILayout.Label ("Log:", EditorStyles.boldLabel);
			_scrollPosition = GUILayout.BeginScrollView (_scrollPosition, GUILayout.Width (360), GUILayout.Height (200));
				GUILayout.Label (Log, _logStyle);
			GUILayout.EndScrollView ();
			if (GUILayout.Button ("Close", GUILayout.Width (100))) {
				Close ();
			}
		}

		void OutputLog (LogData data, string path)
		{
			lock (_sync)
			{
				_log.AppendLine (" " + data.ToString ());
				_updated = true;
			}
		}

		void Update ()
		{
			lock (_sync)
			{
				if (_updated)
				{
					Repaint ();
					_updated = false;
				}
			}
		}
	}
}
