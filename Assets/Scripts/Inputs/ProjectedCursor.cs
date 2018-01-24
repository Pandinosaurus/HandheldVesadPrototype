﻿using NormandErwan.MasterThesis.Experiment.Utilities;
using UnityEngine;

namespace NormandErwan.MasterThesis.Experiment.Inputs
{
  public class ProjectedCursor : MonoBehaviour
  {
    // Editor fields

    [SerializeField]
    private Cursor cursor;

    [SerializeField]
    protected GameObject line;

    // Properties

    public Cursor Cursor { get { return cursor; } set { cursor = value; } }
    public GameObject ProjectionLine { get { return line; } set { line = value; } }
    public bool IsActive { get; protected set; }

    // Variables

    protected Experiment.Task.Grid grid;
    protected GenericVector3<Range<float>> positionRanges = new GenericVector3<Range<float>>();

    // Methods

    protected virtual void Awake()
    {
      grid = transform.parent.GetComponent<Experiment.Task.Grid>();
      grid.Configured += Grid_Configured;
      grid.Zooming += Grid_Zooming;

      positionRanges.X = new Range<float>();
      positionRanges.Y = new Range<float>();
      UpdatePositionRanges();

      SetActive(false);
    }

    protected virtual void OnDestroy()
    {
      grid.Configured -= Grid_Configured;
      grid.Zooming -= Grid_Zooming;
    }

    public virtual void UpdateProjection()
    {
      IsActive = false;
      if (Cursor.IsActivated)
      {
        var projectedPosition = Vector3.ProjectOnPlane(Cursor.transform.position, -grid.transform.forward);
        transform.position = new Vector3(projectedPosition.x, projectedPosition.y, transform.position.z);
        if (positionRanges.X.ContainsValue(transform.localPosition.x) && positionRanges.Y.ContainsValue(transform.localPosition.y))
        {
          IsActive = true;

          float yRotation = (transform.position.z > Cursor.transform.position.z) ? 0f : 180f;
          ProjectionLine.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

          var distance = Vector3.Distance(transform.position, Cursor.transform.position) / 2f / transform.lossyScale.z;
          ProjectionLine.transform.localScale = new Vector3(ProjectionLine.transform.localScale.x, ProjectionLine.transform.localScale.y, distance);
        }
      }
    }

    public virtual void SetActive(bool value)
    {
      IsActive = value;
      foreach (Transform child in transform)
      {
        child.gameObject.SetActive(IsActive);
      }
    }

    protected virtual void Grid_Configured()
    {
      UpdatePositionRanges();
    }

    protected virtual void Grid_Zooming(Interactables.IZoomable grid, Vector3 scaling, Vector3 translation)
    {
      transform.localScale = new Vector3(transform.localScale.x / scaling.x, transform.localScale.y / scaling.y, transform.localScale.z / scaling.z);
      UpdatePositionRanges();
    }

    protected virtual void UpdatePositionRanges()
    {
      positionRanges.X.Minimum = -grid.Scale.x / 2f;
      positionRanges.X.Maximum = grid.Scale.x / 2f;
      positionRanges.Y.Minimum = -grid.Scale.y / 2f;
      positionRanges.Y.Maximum = grid.Scale.y / 2f;
    }
  }
}