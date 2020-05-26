using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

namespace Ash.System.Windows.ShellProvider
{
	// https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Standard/ShellProvider.cs

	public class TaskbarManager
	{
		private readonly ITaskbarList3 taskbarList;

		public bool IsTaskbarSupported => Utilities.IsOSWindows7OrNewer;

		public TaskbarManager()
		{
			if (!IsTaskbarSupported)
			{
				return;
			}

			ITaskbarList _taskbarList = null;

			try
			{
				_taskbarList = (ITaskbarList)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid(CLSID.TaskbarList)));
				_taskbarList.HrInit();

				// This QI will only work on Win7.
				taskbarList = (ITaskbarList3)_taskbarList;
				_taskbarList = null;
			}
			finally
			{
				Utilities.SafeRelease(ref _taskbarList);
			}
		}

		public void SetState(IntPtr windowHandle, TaskbarProgressBarState taskbarState)
		{
			taskbarList?.SetProgressState(windowHandle, taskbarState);
		}

		public void SetValue(IntPtr windowHandle, double progressValue, double progressMax)
		{
			taskbarList?.SetProgressValue(windowHandle, (ulong)progressValue, (ulong)progressMax);
		}
	}

	public enum TaskbarProgressBarState
	{
		None = 0,           //TBPF_NOPROGRESS
		Indeterminate = 1,  //TBPF_INDETERMINATE
		Normal = 2,         //TBPF_NORMAL
		Error = 4,          //TBPF_ERROR
		Paused = 8          //TBPF_PAUSED
	}

	[ComImport, Guid(IID.TaskbarList)]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface ITaskbarList
	{
		void HrInit();

		void AddTab(IntPtr hwnd);

		void DeleteTab(IntPtr hwnd);

		void ActivateTab(IntPtr hwnd);

		void SetActiveAlt(IntPtr hwnd);
	}

	[ComImport, Guid(IID.TaskbarList3)]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface ITaskbarList3
	{
		// ITaskbarList
		[PreserveSig]
		HRESULT HrInit();
		[PreserveSig]
		HRESULT AddTab(IntPtr hwnd);
		[PreserveSig]
		HRESULT DeleteTab(IntPtr hwnd);
		[PreserveSig]
		HRESULT ActivateTab(IntPtr hwnd);
		[PreserveSig]
		HRESULT SetActiveAlt(IntPtr hwnd);

		// ITaskbarList2
		[PreserveSig]
		HRESULT MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

		// ITaskbarList3
		[PreserveSig]
		HRESULT SetProgressValue(IntPtr hwnd, UInt64 ullCompleted, UInt64 ullTotal);
		[PreserveSig]
		HRESULT SetProgressState(IntPtr hwnd, TaskbarProgressBarState state);
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct HRESULT
	{
		[FieldOffset(0)]
		private readonly uint value;

		public HRESULT(uint value)
		{
			this.value = value;
		}

		public int Code => GetCode((int)value);

		public static int GetCode(int error)
		{
			return (int)(error & 0xFFFF);
		}

		public bool Succeeded => (int)value >= 0;

		public bool Failed => (int)value < 0;

		// NOTE: class abbreviated.
	}

	// https://referencesource.microsoft.com/#WindowsBase/Base/MS/Internal/Utilities.cs
	internal static class Utilities
	{
		private static readonly Version _osVersion = Environment.OSVersion.Version;

		internal static bool IsOSVistaOrNewer => _osVersion >= new Version(6, 0);
		internal static bool IsOSWindows7OrNewer => _osVersion >= new Version(6, 1);
		internal static bool IsOSWindows8OrNewer => _osVersion >= new Version(6, 2);

		internal static void SafeDispose<T>(ref T disposable) where T : IDisposable
		{
			// Dispose can safely be called on an object multiple times.
			IDisposable t = disposable;

			disposable = default(T);

			if (t != null)
			{
				t.Dispose();
			}
		}

		[SuppressUnmanagedCodeSecurity, SecurityCritical]
		internal static void SafeRelease<T>(ref T comObject) where T : class
		{
			T t = comObject;

			comObject = default(T);

			if (t != null)
			{
				Debug.Assert(Marshal.IsComObject(t));
				Marshal.ReleaseComObject(t);
			}
		}
	}

	internal static partial class IID
	{
		public const string TaskbarList = "56FDF342-FD6D-11d0-958A-006097C9A090";
		public const string TaskbarList2 = "602D4995-B13A-429b-A66E-1935E44F4317";

		#region Win7 IIDs
		public const string TaskbarList3 = "ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf";
		public const string TaskbarList4 = "c43dc798-95d1-4bea-9030-bb99e2983a1a";
		#endregion
	}

	internal static class CLSID
	{
		public const string TaskbarList = "56FDF344-FD6D-11d0-958A-006097C9A090";
	}
}
