using System;
using System.Diagnostics;
using System.Reflection;

public class DebugUtilitiesTCT
{

    public static string GetClassAndMethodDetails()
    {
        var stackTrace = new StackTrace();

        // Get the current method
        var currentMethod = stackTrace.GetFrame(1).GetMethod();
        var currentClassName = currentMethod.DeclaringType.Name;
        var currentMethodName = currentMethod.Name;

        // Get the caller method (if available)
        string callerDetails = "NoCaller";
        if (stackTrace.FrameCount > 2)
        {
            var callerMethod = stackTrace.GetFrame(2).GetMethod();
            var callerClassName = callerMethod.DeclaringType.Name;
            var callerMethodName = callerMethod.Name;
            callerDetails = $"{callerClassName}.{callerMethodName}";
        }

        // Get the caller's caller method (if available)
        string callerCallerDetails = "NoCallersCaller";
        if (stackTrace.FrameCount > 3)
        {
            var callerCallerMethod = stackTrace.GetFrame(3).GetMethod();
            var callerCallerClassName = callerCallerMethod.DeclaringType.Name;
            var callerCallerMethodName = callerCallerMethod.Name;
            callerCallerDetails = $"{callerCallerClassName}->{callerCallerMethodName}";
        }

        return $"{currentClassName}.{currentMethodName}()  [{callerCallerDetails}->{callerDetails}]";
    }

    public static string GetFullStackTrace()
    {
        var stackTrace = new StackTrace();
        var stackFrameCount = stackTrace.FrameCount;
        string[] methodCalls = new string[stackFrameCount - 2]; // Adjust size to omit the GetFullStackTrace method itself
        string previousClassName = null, ret = "";

        for (int i = 2; i < stackFrameCount; i++) // Start from 2 to skip GetFullStackTrace
        {
            var method = stackTrace.GetFrame(i).GetMethod();
            var className = method.DeclaringType.Name;
            var methodName = method.Name;

            if (className == previousClassName)
            {
                methodCalls[i - 2] = $"{methodName}()"; // Adjust index to fill the correct position
            }
            else
            {
                ret = (i - 2 == 0) ? "\n" : "";
                methodCalls[i - 2] = $"{ret}{className}.{methodName}()"; // Adjust index to fill the correct position
            }

            previousClassName = className;
        }

        Array.Reverse(methodCalls); // Reverse the order of the method calls
        return string.Join("->", methodCalls);
    }


    //------------------
    public static void LogStackTrace()
    {
        string stackTrace = GetFullStackTrace();
        UnityEngine.Debug.Log($"{stackTrace}\n");
    }
}
