using StencilPad.Spatial;

namespace StencilPad.Canvases.Common;

public class LockAxisState
{
    private UnitAxis? _lockedAxis;

    public LockAxisState()
    {
        _lockedAxis = null;
    }

    public void OnDragStart()
    {
        _lockedAxis = null;
    }

    public Unit2D OnDragMove(bool isLockAxisModifier,
                             Unit lockAxisThreshold,
                             Unit2D initialElementPosition,
                             Unit2D targetPosition)
    {
        if (isLockAxisModifier)
        {
            var totalDelta = targetPosition - initialElementPosition;
            
            if (_lockedAxis is null)
            {
                // NOTE: We probably want to set this threshold to the
                // configured size of modifier widgets as that should be
                // loosely equivalent to a deliberate movement by the user.
                if (totalDelta.Magnitude > lockAxisThreshold)
                {
                    if (Unit.Abs(totalDelta.X) > Unit.Abs(totalDelta.Y))
                    {
                        _lockedAxis = UnitAxis.X;
                    }
                    else
                    {
                        _lockedAxis = UnitAxis.Y;
                    }
                }
            }

            if (_lockedAxis is not null)
            {
                if (_lockedAxis == UnitAxis.X)
                {
                    totalDelta = new Unit2D(totalDelta.X, Unit.Zero);
                }
                else
                {
                    totalDelta = new Unit2D(Unit.Zero, totalDelta.Y);
                }

                targetPosition = initialElementPosition + totalDelta;
            }
        }
        else
        {
            _lockedAxis = null;
        }

        return targetPosition;
    }
}
