using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FlameState
{
    NONE,
    HEATING,
    BURNING,
    BURNED_OUT,
    COOLING
}

public interface IFlamable : IQuadTreeObject
{
    List<IFlamable> GetCacheList();

    void SetFlameState(FlameState state);

    FlameState GetFlameState();

    void AddTemperature(float value);

}
