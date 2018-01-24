﻿using DevicesSyncUnity.Messages;
using NormandErwan.MasterThesis.Experiment.Inputs.Interactables;
using System.Collections.Generic;
using UnityEngine;

namespace NormandErwan.MasterThesis.Experiment.Inputs
{
  public class ProjectedCursorsSyncMessage : DevicesSyncMessage
  {
    // Constructors and destructor

    public ProjectedCursorsSyncMessage(int cursorsNumber)
    {
      cursors = new CursorType[cursorsNumber];
      isActive = new bool[cursorsNumber];
      localPositions = new Vector3[cursorsNumber];
    }

    public ProjectedCursorsSyncMessage()
    {
    }

    ~ProjectedCursorsSyncMessage()
    {
    }

    // Properties

    public override int SenderConnectionId { get { return senderConnectionId; } set { senderConnectionId = value; } }
    public override short MessageType { get { return MasterThesis.Experiment.MessageType.ProjectedCursors; } }
    public virtual bool CursorsChanged { get; protected set; }

    // Variables

    public int senderConnectionId;
    public CursorType[] cursors;
    public bool[] isActive;
    public Vector3[] localPositions;

    // Methods

    public void Update(Dictionary<CursorType, ProjectedCursor> projectedCursors)
    {
      CursorsChanged = false;

      for (int i = 0; i < cursors.Length; i++)
      {
        var projectedCursor = projectedCursors[cursors[i]];
        bool wasActive = projectedCursor.IsActive;

        projectedCursor.UpdateProjection();
        isActive[i] = projectedCursor.IsActive;
        
        if (!VectorEquals(localPositions[i], projectedCursor.transform.localPosition))
        {
          CursorsChanged = true;
          localPositions[i] = projectedCursor.transform.localPosition;
        }
        if (wasActive != projectedCursor.IsActive)
        {
          CursorsChanged = true;
          localPositions[i] = Vector3.zero;
        }
      }
    }

    public void Restore(Dictionary<CursorType, ProjectedCursor> projectedCursors)
    {
      for (int i = 0; i < cursors.Length; i++)
      {
        projectedCursors[cursors[i]].SetActive(isActive[i]);
        projectedCursors[cursors[i]].transform.localPosition = localPositions[i];
      }
    }

    protected bool VectorEquals(Vector3 v1, Vector3 v2, float precision = 0.001f)
    {
      return (v1 - v2).sqrMagnitude < (precision * precision);
    }
  }
}