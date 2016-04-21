using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AyoController
{
    public enum EventType
    {
        MouseOut,
        MouseClick,
        MouseOver,
    }
    public class EventSystem
    {
        public EventType Type;
        public string ControlId;
    }
    public class MlScript
    {
        List<EventSystem> _events = new List<EventSystem>();
    }
}
