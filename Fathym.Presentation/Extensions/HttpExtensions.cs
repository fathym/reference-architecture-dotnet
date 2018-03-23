﻿using Fathym.Fabric;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fathym
{
	public static class HttpExtensions
	{
		public static string GetFullUrl(this HttpRequest request)
		{
			if (request.Path == "/")
				return $"{request.Scheme}://{request.Host.Value}";

			return $"{request.Scheme}://{request.Host.Value}{request.Path}";
		}

		public static string GetUserAgent(this HttpRequest request)
		{
			return request.Headers.ContainsKey("User-Agent") ? (string)request.Headers["User-Agent"] : string.Empty;
		}

		public static TContext ResolveContext<TContext>(this HttpContext context, string contextLookup)
		{
			return (TContext)context.Items[contextLookup];
		}

		public static async Task<Status> WithSessionLockedContext<TContext>(this HttpContext context, string contextLookup, SemaphoreSlim readLock, Func<Task<TContext>> action)
		{
			if (!context.Session.Keys.Contains(contextLookup))
			{
				var status = await WithLockedContext(context, contextLookup, readLock, action);

				if (status)
					context.Session.Set(contextLookup, context.Items[contextLookup].ToBytes());

				return status;
			}
			else
			{
				context.Items[contextLookup] = (TContext)context.Session.Get(contextLookup).FromBytes();

				return Status.Success;
			}
		}

		public static async Task<Status> WithLockedContext<TContext>(this HttpContext context, string contextLookup, SemaphoreSlim readLock, Func<Task<TContext>> action)
		{
			try
			{
				await readLock.WaitAsync();

				var ctxt = action();

				if (ctxt != null)
				{
					context.Items[contextLookup] = ctxt;

					return Status.Success;
				}
				else
					throw new Exception("The enterprise could not be loaded for the host.");
			}
			catch (Exception ex)
			{
				FabricEventSource.Current.Exception(ex.ToString());

				throw;
			}
			finally
			{
				readLock.Release();
			}
		}
	}
}
