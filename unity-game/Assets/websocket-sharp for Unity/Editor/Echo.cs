using System;
using UnityEngine;
using WebSocketSharp.Server;

namespace WebSocketSharp.Unity.Editor
{
	public class Echo : WebSocketService
	{
		protected override void OnMessage (MessageEventArgs e)
		{
			Send (e.Data);
		}
	}
}
