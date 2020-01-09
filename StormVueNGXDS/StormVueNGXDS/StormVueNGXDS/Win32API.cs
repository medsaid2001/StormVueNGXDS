using System;
using System.Runtime.InteropServices;

namespace StormVue2RTCM
{
	internal class Win32API
	{
		[StructLayout(LayoutKind.Sequential)]
		public class SECURITY_ATTRIBUTES
		{
			private uint nLegnth;

			private int lpSecurityDescriptor;

			[MarshalAs(UnmanagedType.VariantBool)]
			private bool bInheritHandle;
		}

		public const int PAGE_READWRITE = 4;

		public const int PAGE_READONLY = 2;

		public const int FILE_MAP_ALL_ACCESS = 983071;

		public const int FILE_MAP_READ = 4;

		public const int INVALID_HANDLE_VALUE = -1;

		public const int ERROR_ALREADY_EXISTS = 183;

		[DllImport("Kernel32.dll", SetLastError = true)]
		public static extern IntPtr CreateFileMappingW(int hFile, IntPtr lpAttributes, int flProtect, int dwMaximumSizeHigh, int dwMaximumSizeLow, [MarshalAs(UnmanagedType.LPWStr)] string lpName);

		[DllImport("Kernel32.dll")]
		public static extern int CloseHandle(IntPtr hObject);

		[DllImport("Kernel32.dll", SetLastError = true)]
		public static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, int dwDesiredAccess, int dwFileOffsetHigh, int dwFileOffsetLow, int dwNumberOfBytesToMap);

		[DllImport("Kernel32.dll")]
		public static extern int UnmapViewOfFile(IntPtr lpBaseAddress);

		[DllImport("Kernel32.dll", SetLastError = true)]
		public static extern IntPtr CreateSemaphoreA(SECURITY_ATTRIBUTES lpSemaphoreAttributes, int lInitialCount, int lMaximumCount, string lpName);

		[DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern int ReleaseSemaphore(IntPtr hSemaphore, int lReleaseCount, out int lpPreviousCount);
	}
}
