using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ChunkLinkedList
{
    private Chunk head;
    private Chunk tail;
    private int count;

    public ChunkLinkedList()
    {

    }

    public int Count()
    {
        return count;
    }

    public Chunk Head()
    {
        return head;
    }

    public void Add(Chunk c)
    {
        if (head == null)
        {
            head = tail = c;
        }else
        {
            tail.next = c;
            c.previous = tail;
            tail = c;

        }
        count++;
    }

    public void Remove(Chunk c)
    {
        if (c.next == null)
        {
            if (c.previous == null)
            {
                tail = head = null;
            }else
            {
                tail = c.previous;
                tail.next = null;
            }
        }else
        {
            if (c.previous == null)
            {
                head = c.next;
                head.previous = null;
            }else
            {
                c.next.previous = c.previous;
                c.previous.next = c.next;
            }
        }
        c.previous = null;
        c.next = null;
        count--;
    }
}