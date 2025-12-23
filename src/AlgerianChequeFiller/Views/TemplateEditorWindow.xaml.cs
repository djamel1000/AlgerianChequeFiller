using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using AlgerianChequeFiller.Models;
using AlgerianChequeFiller.Services;
using AlgerianChequeFiller.ViewModels;

namespace AlgerianChequeFiller.Views;

/// <summary>
/// Template editor window with draggable field rectangles.
/// </summary>
public partial class TemplateEditorWindow : Window
{
    private readonly TemplateEditorViewModel _viewModel;
    private readonly Dictionary<string, Border> _fieldElements = new();
    private Border? _draggedElement;
    private Point _dragStartPoint;
    private Point _elementStartPosition;
    private bool _isDragging;
    private bool _isResizing;
    private const double ScaleFactor = 3.78; // mm to canvas pixels (approx)

    private static readonly Dictionary<string, Color> FieldColors = new()
    {
        ["AmountNumeric"] = Color.FromRgb(59, 130, 246),   // Blue
        ["AmountWordsL1"] = Color.FromRgb(34, 197, 94),    // Green
        ["AmountWordsL2"] = Color.FromRgb(34, 197, 94),    // Green
        ["Beneficiary"] = Color.FromRgb(168, 85, 247),     // Purple
        ["Place"] = Color.FromRgb(245, 158, 11),           // Orange
        ["Date"] = Color.FromRgb(239, 68, 68)              // Red
    };

    public TemplateEditorWindow()
    {
        InitializeComponent();

        _viewModel = (TemplateEditorViewModel)DataContext;
        _viewModel.TemplateChanged += OnTemplateChanged;
        _viewModel.SaveRequested += OnSaveRequested;

        Loaded += OnLoaded;
    }

    private void OnSaveRequested(object? sender, EventArgs e)
    {
        var currentName = _viewModel.Template.TemplateName ?? "Mon Modèle";
        var dialog = new SaveTemplateDialog(currentName);
        dialog.Owner = this;

        if (dialog.ShowDialog() == true)
        {
            _viewModel.SaveWithName(dialog.TemplateName);
            MessageBox.Show($"Modèle '{dialog.TemplateName}' enregistré avec succès!", 
                "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RefreshCanvas();
    }

    private void OnTemplateChanged(object? sender, EventArgs e)
    {
        RefreshCanvas();
    }

    private void RefreshCanvas()
    {
        TemplateCanvas.Children.Clear();
        _fieldElements.Clear();

        // Draw cheque outline
        var chequeWidth = _viewModel.Template.ChequeSizeMm.Width * ScaleFactor;
        var chequeHeight = _viewModel.Template.ChequeSizeMm.Height * ScaleFactor;

        TemplateCanvas.Width = chequeWidth;
        TemplateCanvas.Height = chequeHeight;

        // Add cheque background image
        try
        {
            var backgroundImage = new System.Windows.Controls.Image
            {
                Width = chequeWidth,
                Height = chequeHeight,
                Source = new System.Windows.Media.Imaging.BitmapImage(
                    new Uri("pack://application:,,,/Resources/cheque_background.png")),
                Stretch = Stretch.Fill
            };
            TemplateCanvas.Children.Add(backgroundImage);
        }
        catch
        {
            // Fallback to white rectangle if image fails to load
            var outline = new Rectangle
            {
                Width = chequeWidth,
                Height = chequeHeight,
                Stroke = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 5, 3 },
                Fill = Brushes.White
            };
            TemplateCanvas.Children.Add(outline);
        }

        // Draw field rectangles
        foreach (var (fieldId, fieldRect) in _viewModel.Template.Fields)
        {
            CreateFieldElement(fieldId, fieldRect);
        }
    }

    private void CreateFieldElement(string fieldId, FieldRect fieldRect)
    {
        var color = FieldColors.GetValueOrDefault(fieldId, Colors.Gray);

        var border = new Border
        {
            Width = fieldRect.W * ScaleFactor,
            Height = fieldRect.H * ScaleFactor,
            Background = new SolidColorBrush(Color.FromArgb(40, color.R, color.G, color.B)),
            BorderBrush = new SolidColorBrush(color),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(3),
            Cursor = Cursors.SizeAll,
            Tag = fieldId
        };

        // Add label
        var label = new TextBlock
        {
            Text = GetFieldLabel(fieldId),
            FontSize = 10,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(color),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };

        // Add resize handle (larger, more visible)
        var resizeHandle = new Rectangle
        {
            Width = 14,
            Height = 14,
            Fill = new SolidColorBrush(color),
            Stroke = Brushes.White,
            StrokeThickness = 2,
            Cursor = Cursors.SizeNWSE,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 2, 2),
            Tag = "resize"
        };

        // Create grid to hold both label and resize handle
        var grid = new Grid();
        grid.Children.Add(label);
        grid.Children.Add(resizeHandle);
        border.Child = grid;

        // Position on canvas
        Canvas.SetLeft(border, fieldRect.X * ScaleFactor);
        Canvas.SetTop(border, fieldRect.Y * ScaleFactor);

        // Event handlers
        border.MouseLeftButtonDown += OnFieldMouseDown;
        border.MouseMove += OnFieldMouseMove;
        border.MouseLeftButtonUp += OnFieldMouseUp;

        resizeHandle.MouseLeftButtonDown += OnResizeHandleMouseDown;

        TemplateCanvas.Children.Add(border);
        _fieldElements[fieldId] = border;
    }

    private string GetFieldLabel(string fieldId) => fieldId switch
    {
        "AmountNumeric" => "Montant",
        "AmountWordsL1" => "Lettres L1",
        "AmountWordsL2" => "Lettres L2",
        "Beneficiary" => "Bénéficiaire",
        "Place" => "Lieu",
        "Date" => "Date",
        _ => fieldId
    };

    private void OnFieldMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border border || border.Tag is not string fieldId)
            return;

        // Check if clicking on resize handle
        if (e.OriginalSource is Rectangle rect && rect.Tag as string == "resize")
            return;

        _viewModel.SelectedFieldId = fieldId;
        _draggedElement = border;
        _dragStartPoint = e.GetPosition(TemplateCanvas);
        _elementStartPosition = new Point(Canvas.GetLeft(border), Canvas.GetTop(border));
        _isDragging = true;
        border.CaptureMouse();

        // Highlight selected
        UpdateFieldHighlights(fieldId);

        e.Handled = true;
    }

    private void OnFieldMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || _draggedElement == null)
            return;

        var currentPoint = e.GetPosition(TemplateCanvas);
        var deltaX = currentPoint.X - _dragStartPoint.X;
        var deltaY = currentPoint.Y - _dragStartPoint.Y;

        var newX = Math.Max(0, Math.Min(_elementStartPosition.X + deltaX, 
            TemplateCanvas.Width - _draggedElement.Width));
        var newY = Math.Max(0, Math.Min(_elementStartPosition.Y + deltaY, 
            TemplateCanvas.Height - _draggedElement.Height));

        Canvas.SetLeft(_draggedElement, newX);
        Canvas.SetTop(_draggedElement, newY);
    }

    private void OnFieldMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging || _draggedElement == null)
            return;

        var fieldId = _draggedElement.Tag as string;
        if (fieldId != null)
        {
            var newX = Canvas.GetLeft(_draggedElement) / ScaleFactor;
            var newY = Canvas.GetTop(_draggedElement) / ScaleFactor;
            _viewModel.UpdateFieldPosition(fieldId, newX, newY);
        }

        _draggedElement.ReleaseMouseCapture();
        _isDragging = false;
        _draggedElement = null;
    }

    private void OnResizeHandleMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Rectangle handle)
            return;

        var border = handle.Parent is Grid grid ? grid.Parent as Border : null;
        if (border?.Tag is not string fieldId)
            return;

        _viewModel.SelectedFieldId = fieldId;
        _draggedElement = border;
        _dragStartPoint = e.GetPosition(TemplateCanvas);
        _elementStartPosition = new Point(border.Width, border.Height);
        _isResizing = true;
        handle.CaptureMouse();

        UpdateFieldHighlights(fieldId);

        e.Handled = true;

        // Handle resize in mouse move
        handle.MouseMove += OnResizeMouseMove;
        handle.MouseLeftButtonUp += OnResizeMouseUp;
    }

    private void OnResizeMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isResizing || _draggedElement == null)
            return;

        var currentPoint = e.GetPosition(TemplateCanvas);
        var deltaX = currentPoint.X - _dragStartPoint.X;
        var deltaY = currentPoint.Y - _dragStartPoint.Y;

        var newWidth = Math.Max(20, _elementStartPosition.X + deltaX);
        var newHeight = Math.Max(10, _elementStartPosition.Y + deltaY);

        _draggedElement.Width = newWidth;
        _draggedElement.Height = newHeight;
    }

    private void OnResizeMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isResizing || _draggedElement == null)
            return;

        var fieldId = _draggedElement.Tag as string;
        if (fieldId != null)
        {
            var newWidth = _draggedElement.Width / ScaleFactor;
            var newHeight = _draggedElement.Height / ScaleFactor;
            _viewModel.UpdateFieldSize(fieldId, newWidth, newHeight);
        }

        if (sender is Rectangle handle)
        {
            handle.ReleaseMouseCapture();
            handle.MouseMove -= OnResizeMouseMove;
            handle.MouseLeftButtonUp -= OnResizeMouseUp;
        }

        _isResizing = false;
        _draggedElement = null;
    }

    private void UpdateFieldHighlights(string selectedFieldId)
    {
        foreach (var (fieldId, border) in _fieldElements)
        {
            var color = FieldColors.GetValueOrDefault(fieldId, Colors.Gray);
            border.BorderThickness = fieldId == selectedFieldId 
                ? new Thickness(3) 
                : new Thickness(2);
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
