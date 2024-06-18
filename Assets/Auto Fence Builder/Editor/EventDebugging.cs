using UnityEngine;

public static class EventDebugging
{
    public static EventType MouseButtonActivityDetected(Event currentEvent, bool mouseDownOnly = false)
    {
        if (!currentEvent.isMouse)
            return EventType.Ignore;

        if (mouseDownOnly && currentEvent.type != EventType.MouseDown)
            return EventType.Ignore;

        Debug.Log($"Mouse event detected: {currentEvent.type}\n");

        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                HandleMouseDown(currentEvent);
                return currentEvent.type;

            case EventType.MouseDrag:
                HandleMouseDrag(currentEvent);
                return currentEvent.type;

            case EventType.MouseUp:
                HandleMouseUp(currentEvent);
                return currentEvent.type;

            default:
                return EventType.Ignore;
        }
    }

    private static void HandleMouseDown(Event currentEvent)
    {
        switch (currentEvent.button)
        {
            case 0:
                Debug.Log("Mouse Down - Left Button\n");
                break;

            case 1:
                Debug.Log("Mouse Down - Right Button\n");
                break;

            default:
                Debug.Log($"Mouse Down - Button {currentEvent.button}\n");
                break;
        }
    }

    private static void HandleMouseDrag(Event currentEvent)
    {
        switch (currentEvent.button)
        {
            case 0:
                Debug.Log("Mouse Dragging - Left Button\n");
                break;

            case 1:
                Debug.Log("Mouse Dragging - Right Button\n");
                break;

            default:
                Debug.Log($"Mouse Dragging - Button {currentEvent.button}\n");
                break;
        }
    }

    private static void HandleMouseUp(Event currentEvent)
    {
        switch (currentEvent.button)
        {
            case 0:
                Debug.Log("Mouse Up - Left Button\n");
                break;

            case 1:
                Debug.Log("Mouse Up - Right Button\n");
                break;

            default:
                Debug.Log($"Mouse Up - Button {currentEvent.button}\n");
                break;
        }
    }

    public static EventType MouseOtherActivityDetected(Event currentEvent)
    {
        if (!currentEvent.isMouse)
            return EventType.Ignore;

        Debug.Log($"Other mouse event detected: {currentEvent.type}\n");

        switch (currentEvent.type)
        {
            case EventType.MouseMove:
                Debug.Log("Mouse Move\n");
                return currentEvent.type;

            case EventType.ScrollWheel:
                Debug.Log("Scroll Wheel\n");
                return currentEvent.type;

                // No default case needed, since we're filtering out non-mouse events
        }

        return EventType.Ignore;
    }

    public static EventType LogEvent(Event currentEvent, bool ignoreIgnoreEvents = true)
    {
        if (currentEvent.type == EventType.Ignore && ignoreIgnoreEvents)
            return EventType.Ignore;

        Debug.Log($"Event: {currentEvent.type} \n");
        return currentEvent.type;
    }
}