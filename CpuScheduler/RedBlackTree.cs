using System.Security.Cryptography.X509Certificates;
using UtilClasses;

public enum RBColor { Red, Black }

public class RBNode
{
    public (double vruntime, string pid) Key;
    public ProcessData Value;
    public RBNode? Left, Right, Parent;
    public RBColor Color;

    public RBNode((double, string) key, ProcessData value)
    {
        Key = key;
        Value = value;
        Color = RBColor.Red;
    }
}

public class RBTree
{
    private RBNode? root;

    // Public APIs
    public bool IsEmpty => root == null;

    public void Insert(double vruntime, string pid, ProcessData proc)
    {
        var node = new RBNode((vruntime, pid), proc);
        InsertNode(node);
    }

    public (double vruntime, string pid, ProcessData proc)? PopMin()
    {
        var n = Minimum(root);
        if (n == null) return null;
        var key = n.Key;
        var value = n.Value;
        DeleteNode(n);
        return (key.vruntime, key.pid, value);
    }

    // --- internal helpers ---

    private RBNode? Minimum(RBNode? node)
    {
        if (node == null) return null;
        while (node.Left != null) node = node.Left;
        return node;
    }

    private void InsertNode(RBNode z)
    {
        RBNode? y = null;
        var x = root;
        while (x != null)
        {
            y = x;
            if (Compare(z.Key, x.Key) < 0) x = x.Left;
            else x = x.Right;
        }
        z.Parent = y;
        if (y == null) root = z;
        else if (Compare(z.Key, y.Key) < 0) y.Left = z;
        else y.Right = z;

        z.Left = null;
        z.Right = null;
        z.Color = RBColor.Red;
        InsertFixup(z);
    }

    private void InsertFixup(RBNode z)
    {
        while (z.Parent != null && z.Parent.Color == RBColor.Red)
        {
            if (z.Parent == z.Parent.Parent?.Left)
            {
                var y = z.Parent.Parent.Right;
                if (y != null && y.Color == RBColor.Red)
                {
                    z.Parent.Color = RBColor.Black;
                    y.Color = RBColor.Black;
                    z.Parent.Parent.Color = RBColor.Red;
                    z = z.Parent.Parent;
                }
                else
                {
                    if (z == z.Parent.Right)
                    {
                        z = z.Parent;
                        LeftRotate(z);
                    }
                    z.Parent!.Color = RBColor.Black;
                    z.Parent.Parent!.Color = RBColor.Red;
                    RightRotate(z.Parent.Parent);
                }
            }
            else
            {
                var y = z.Parent.Parent?.Left;
                if (y != null && y.Color == RBColor.Red)
                {
                    z.Parent.Color = RBColor.Black;
                    y.Color = RBColor.Black;
                    z.Parent.Parent!.Color = RBColor.Red;
                    z = z.Parent.Parent;
                }
                else
                {
                    if (z == z.Parent.Left)
                    {
                        z = z.Parent;
                        RightRotate(z);
                    }
                    z.Parent!.Color = RBColor.Black;
                    z.Parent.Parent!.Color = RBColor.Red;
                    LeftRotate(z.Parent.Parent);
                }
            }
        }
        root!.Color = RBColor.Black;
    }

    private void LeftRotate(RBNode x)
    {
        var y = x.Right;
        if (y == null) return;
        x.Right = y.Left;
        if (y.Left != null) y.Left.Parent = x;
        y.Parent = x.Parent;
        if (x.Parent == null) root = y;
        else if (x == x.Parent.Left) x.Parent.Left = y;
        else x.Parent.Right = y;
        y.Left = x;
        x.Parent = y;
    }

    private void RightRotate(RBNode x)
    {
        var y = x.Left;
        if (y == null) return;
        x.Left = y.Right;
        if (y.Right != null) y.Right.Parent = x;
        y.Parent = x.Parent;
        if (x.Parent == null) root = y;
        else if (x == x.Parent.Right) x.Parent.Right = y;
        else x.Parent.Left = y;
        y.Right = x;
        x.Parent = y;
    }

    private void Transplant(RBNode u, RBNode? v)
    {
        if (u.Parent == null) root = v;
        else if (u == u.Parent.Left) u.Parent.Left = v;
        else u.Parent.Right = v;
        if (v != null) v.Parent = u.Parent;
    }

    private void DeleteNode(RBNode z)
    {
        var y = z;
        var yOriginalColor = y.Color;
        RBNode? x;
        if (z.Left == null)
        {
            x = z.Right;
            Transplant(z, z.Right);
        }
        else if (z.Right == null)
        {
            x = z.Left;
            Transplant(z, z.Left);
        }
        else
        {
            y = Minimum(z.Right)!;
            yOriginalColor = y.Color;
            x = y.Right;
            if (y.Parent == z)
            {
                if (x != null) x.Parent = y;
            }
            else
            {
                Transplant(y, y.Right);
                y.Right = z.Right;
                if (y.Right != null) y.Right.Parent = y;
            }
            Transplant(z, y);
            y.Left = z.Left;
            if (y.Left != null) y.Left.Parent = y;
            y.Color = z.Color;
        }
        if (yOriginalColor == RBColor.Black)
            DeleteFixup(x);
    }

    private void DeleteFixup(RBNode? x)
    {
        while (x != root && (x == null || x.Color == RBColor.Black))
        {
            if (x == x?.Parent?.Left)
            {
                var w = x?.Parent?.Right;
                if (w != null && w.Color == RBColor.Red)
                {
                    w.Color = RBColor.Black;
                    x!.Parent!.Color = RBColor.Red;
                    LeftRotate(x.Parent);
                    w = x.Parent.Right;
                }
                if ((w?.Left == null || w.Left.Color == RBColor.Black) &&
                    (w?.Right == null || w.Right.Color == RBColor.Black))
                {
                    if (w != null) w.Color = RBColor.Red;
                    x = x?.Parent;
                }
                else
                {
                    if (w?.Right == null || w.Right.Color == RBColor.Black)
                    {
                        if (w?.Left != null) w.Left.Color = RBColor.Black;
                        if (w != null) w.Color = RBColor.Red;
                        RightRotate(w!);
                        w = x?.Parent?.Right;
                    }
                    if (w != null) w.Color = x!.Parent!.Color;
                    if (x?.Parent != null) x.Parent.Color = RBColor.Black;
                    if (w?.Right != null) w.Right.Color = RBColor.Black;
                    if (x?.Parent != null) LeftRotate(x.Parent);
                    x = root;
                }
            }
            else
            {
                var w = x?.Parent?.Left;
                if (w != null && w.Color == RBColor.Red)
                {
                    w.Color = RBColor.Black;
                    x!.Parent!.Color = RBColor.Red;
                    RightRotate(x.Parent);
                    w = x.Parent.Left;
                }
                if ((w?.Right == null || w.Right.Color == RBColor.Black) &&
                    (w?.Left == null || w.Left.Color == RBColor.Black))
                {
                    if (w != null) w.Color = RBColor.Red;
                    x = x?.Parent;
                }
                else
                {
                    if (w?.Left == null || w.Left.Color == RBColor.Black)
                    {
                        if (w?.Right != null) w.Right.Color = RBColor.Black;
                        if (w != null) w.Color = RBColor.Red;
                        LeftRotate(w!);
                        w = x?.Parent?.Left;
                    }
                    if (w != null) w.Color = x!.Parent!.Color;
                    if (x?.Parent != null) x.Parent.Color = RBColor.Black;
                    if (w?.Left != null) w.Left.Color = RBColor.Black;
                    if (x?.Parent != null) RightRotate(x.Parent);
                    x = root;
                }
            }
        }
        if (x != null) x.Color = RBColor.Black;
    }

    // comparator: first compare vruntime, then pid to break ties deterministically
    private int Compare((double vruntime, string pid) a, (double vruntime, string pid) b)
    {
        int cmp = a.vruntime.CompareTo(b.vruntime);
        if (cmp != 0) return cmp;
        return string.CompareOrdinal(a.pid, b.pid);
    }
}