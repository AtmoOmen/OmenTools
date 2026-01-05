using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace OmenTools.Helpers;

public static unsafe class MarkingControllerExtensions
{
    extension(scoped ref MarkingController controller)
    {
        #region 属性

        public Vector3 GetFieldMarkerPosition(scoped in FieldMarkerPoint point)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>((uint)point, 7);

            fixed (MarkingController* controllerPtr = &controller)
            {
                ArgumentNullException.ThrowIfNull(controllerPtr);
                
                return controllerPtr->FieldMarkers[(int)point].Position;
            }
        }

        public bool IsFieldMarkerActive(scoped in FieldMarkerPoint point)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>((uint)point, 7);

            fixed (MarkingController* controllerPtr = &controller)
            {
                ArgumentNullException.ThrowIfNull(controllerPtr);
                
                return controllerPtr->FieldMarkers[(int)point].Active;
            }
        }

        #endregion
        
        public byte SetFieldMarker(scoped in FieldMarkerPoint point, scoped in Vector3 pos, scoped in bool isActive)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>((uint)point, 7);

            fixed (MarkingController* controllerPtr = &controller)
            {
                ArgumentNullException.ThrowIfNull(controllerPtr);

                fixed (Vector3* posPtr = &pos)
                {
                    return isActive ? controllerPtr->PlaceFieldMarker((uint)point, posPtr) : controllerPtr->ClearFieldMarker((uint)point);
                }
            }
        }
        
        public void SetFieldMarkerLocal(scoped in FieldMarkerPoint point, scoped in Vector3 pos, scoped in bool isActive)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>((uint)point, 7);

            fixed (MarkingController* controllerPtr = &controller)
            {
                ArgumentNullException.ThrowIfNull(controllerPtr);

                var marker      = (FieldMarker*)Unsafe.AsPointer(ref controllerPtr->FieldMarkers[(int)point]);
                var markAddress = (nint)marker;
                if (!isActive)
                    MemoryHelper.Write(markAddress + 28, (byte)0);
                else
                {
                    MemoryHelper.Write(markAddress,     pos.X);
                    MemoryHelper.Write(markAddress + 4, pos.Y);
                    MemoryHelper.Write(markAddress + 8, pos.Z);

                    MemoryHelper.Write(markAddress + 16, (int)(pos.X * 1000));
                    MemoryHelper.Write(markAddress + 20, (int)(pos.Y * 1000));
                    MemoryHelper.Write(markAddress + 24, (int)(pos.Z * 1000));

                    MemoryHelper.Write(markAddress + 28, (byte)1);
                }
            }
        }

        #region 放置

        public byte PlaceFieldMarker(scoped in FieldMarkerPoint point, scoped in Vector3 pos)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>((uint)point, 7);

            fixed (MarkingController* controllerPtr = &controller)
            {
                ArgumentNullException.ThrowIfNull(controllerPtr);

                return controllerPtr->SetFieldMarker(point, pos, true);
            }
        }
        
        public void PlaceFieldMarkerLocal(scoped in FieldMarkerPoint point, scoped in Vector3 pos)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>((uint)point, 7);

            fixed (MarkingController* controllerPtr = &controller)
            {
                ArgumentNullException.ThrowIfNull(controllerPtr);

                controllerPtr->SetFieldMarkerLocal(point, pos, true);
            }
        }
        
        public void PlaceFieldMarkerOnline(scoped in FieldMarkerPoint point, scoped in Vector3 pos)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>((uint)point, 7);
            
            ExecuteCommandManager.ExecuteCommand(ExecuteCommandFlag.PlaceFieldMarker, 
                                                 (uint)point, 
                                                 (uint)(int)(pos.X * 1000f), 
                                                 (uint)(int)(pos.Y * 1000f), 
                                                 (uint)(int)(pos.Z * 1000f));
        }

        #endregion

        #region 清除
        
        public byte ClearFieldMarker(scoped in FieldMarkerPoint point)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>((uint)point, 7);

            fixed (MarkingController* controllerPtr = &controller)
            {
                ArgumentNullException.ThrowIfNull(controllerPtr);

                return controllerPtr->SetFieldMarker(point, Vector3.Zero, false);
            }
        }
        
        public void ClearFieldMarkerLocal(scoped in FieldMarkerPoint point)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>((uint)point, 7);

            fixed (MarkingController* controllerPtr = &controller)
            {
                ArgumentNullException.ThrowIfNull(controllerPtr);

                controllerPtr->SetFieldMarkerLocal(point, Vector3.Zero, false);
            }
        }

        public void ClearFieldMarkerOnline(scoped in FieldMarkerPoint point)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>((uint)point, 7);
            
            ExecuteCommandManager.ExecuteCommand(ExecuteCommandFlag.RemoveFieldMarker, (uint)point);
        }
        
        public void ClearFieldMarkers() =>
            Enum.GetValues<FieldMarkerPoint>().ForEach(x => x.Clear());
        
        public void ClearFieldMarkersLocal() =>
            Enum.GetValues<FieldMarkerPoint>().ForEach(x => x.ClearLocal());
        
        public void ClearFieldMarkersOnline() =>
            Enum.GetValues<FieldMarkerPoint>().ForEach(x => x.ClearOnline());

        #endregion
    }

    extension(FieldMarkerPoint point)
    {
        public Vector3 GetPosition() => 
            MarkingController.Instance()->GetFieldMarkerPosition(point);

        public bool IsActive() =>
            MarkingController.Instance()->IsFieldMarkerActive(point);

        public byte Set(Vector3 pos, bool isActive) => 
            MarkingController.Instance()->SetFieldMarker(point, pos, isActive);
        
        public void SetLocal(Vector3 pos, bool isActive) => 
            MarkingController.Instance()->SetFieldMarkerLocal(point, pos, isActive);

        public byte Place(Vector3 pos) => 
            MarkingController.Instance()->PlaceFieldMarker(point, pos);
        
        public void PlaceLocal(Vector3 pos) => 
            MarkingController.Instance()->PlaceFieldMarkerLocal(point, pos);

        public void PlaceOnline(Vector3 pos) => 
            MarkingController.Instance()->PlaceFieldMarkerOnline(point, pos);

        public byte Clear() => 
            MarkingController.Instance()->ClearFieldMarker(point);
        
        public void ClearLocal() => 
            MarkingController.Instance()->ClearFieldMarkerLocal(point);

        public void ClearOnline() => 
            MarkingController.Instance()->ClearFieldMarkerOnline(point);
    }
}

public enum FieldMarkerPoint : uint
{
    A,
    B,
    C,
    D,
    One,
    Two,
    Three,
    Four
}
