using System;
using System.Threading;
using UnityEngine;
using WebSocketSharp.Server;

namespace WebSocketSharp.Unity.Editor
{
	public class Chat : WebSocketService
	{
		static int _num = 0;

		string _name;

		string GetName ()
		{
			string name;
			return (name = QueryString ["name"]).IsNullOrEmpty ()
			       ? "anon#" + GetNum ()
			       : name;
		}

		int GetNum ()
		{
			return Interlocked.Increment (ref _num);
		}

		protected override void OnOpen ()
		{
			_name = GetName ();
		}

		protected override void OnMessage (MessageEventArgs e)
		{
			var msg = String.Format ("{0}: {1}", _name, e.Data);
			Broadcast (msg);
		}

		protected override void OnClose (CloseEventArgs e)
		{
			var msg = String.Format ("{0} got logged off...", _name);
			Broadcast (msg);
		}
	}
}
