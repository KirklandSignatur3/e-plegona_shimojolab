using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTrack : MonoBehaviour
{
    [SerializeField] public List<Note> notes = new List<Note>();

    private void OnDestroy() 
    {
        notes.Clear();    
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
    public Note GetNote(int index)
    {
        return notes[index];
    }

    public List<Note> GetNotes()
    {
        return notes;
    }

    public void AddNote(Note note)
    {
        notes.Add(note);
    }

    public void RemoveNote(Note note)
    {
        notes.Remove(note);
    }
}
