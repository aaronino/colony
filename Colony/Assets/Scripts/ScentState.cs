using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public enum ScentState
{
    /// <summary>
    /// Scent is losing strength.
    /// </summary>
    Fading, 

    /// <summary>
    /// Scent has spread this turn and will not spread any further.
    /// </summary>
    Holding,

    /// <summary>
    /// Scent will spread to adjacent areas this turn.
    /// </summary>
    Spreading
}
