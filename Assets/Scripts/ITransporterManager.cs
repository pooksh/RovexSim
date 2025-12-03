using System.Collections.Generic;
using UnityEngine;

public interface ITransporterManager
{
    List<GameObject> GetTransporters();
    bool IsInitialized();
}



