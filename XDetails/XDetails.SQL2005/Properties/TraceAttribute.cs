using System;
using System.Diagnostics;
using PostSharp.Aspects;

namespace XDetails.Properties
{
	/// <summary>
	/// Aspect that, when applied on a method, emits a trace message before and
	/// after the method execution.
	/// </summary>
	[Serializable]
	public sealed class TraceAttribute : OnMethodBoundaryAspect
	{
		private const string _timeFormat = "HH:mm:ss.fff";

		/// <summary>
		/// Method invoked before the execution of the method to which the current
		/// aspect is applied.
		/// </summary>
		/// <param name="args">Information about the method being executed.</param>
		public override void OnEntry(MethodExecutionArgs args)
		{
			Trace.TraceInformation
				(	"{2} {0}.{1}: Enter",
					args.Method.DeclaringType.FullName, 
					args.Method.Name,
					DateTime.Now.ToString(_timeFormat)
				);
			Trace.Indent();
		}

		/// <summary>
		/// Method invoked after successfull execution of the method to which the current
		/// aspect is applied.
		/// </summary>
		/// <param name="args">Information about the method being executed.</param>
		public override void OnSuccess(MethodExecutionArgs args)
		{
			Trace.Unindent();
			Trace.TraceInformation
			(	"{2} {0}.{1}: Success",
				args.Method.DeclaringType.FullName, args.Method.Name,
				DateTime.Now.ToString(_timeFormat)
			);
		}

		/// <summary>
		/// Method invoked after failure of the method to which the current
		/// aspect is applied.
		/// </summary>
		/// <param name="args">Information about the method being executed.</param>
		public override void OnException(MethodExecutionArgs args)
		{
			Trace.Unindent();
			Trace.TraceInformation("{3} {0}.{1}: Exception {2}",
				args.Method.DeclaringType.FullName, args.Method.Name,
				args.Exception.Message,
				DateTime.Now.ToString(_timeFormat)
			);
		}
	}
}