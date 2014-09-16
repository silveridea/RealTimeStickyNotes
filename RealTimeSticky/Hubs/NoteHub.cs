using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using signalr = Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using System.Data;
using System.Web.Mvc;
using dal = RealTimeSticky.DAL;
using System.Data.Entity;

namespace RealTimeSticky
{
    public class NoteHub : Hub
    {
        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        public void GetNotes()
        {
            using (var db = new dal.RTSEntities())
            {
                var notes = db.Notes.ToList();
                Clients.All.AddNotesServer(notes);
            }
        }

        public void AddNewNote()
        {
            using (var db = new dal.RTSEntities())
            {
                var note = new dal.Note
                    {
                        CreatedOn = DateTime.UtcNow,
                        Left = 15,
                        Top = 52
                    };

                db.Notes.Add(note);
                db.SaveChanges();
                Clients.All.AddNoteServer(note);
            }
        }

        public void UpdateNote(dal.Note note)
        {
            if (note != null)
            {
                using (var db = new dal.RTSEntities())
                {
                    db.Notes.Attach(note);
                    db.Entry(note).State = EntityState.Modified;
                    db.SaveChanges();
                    Clients.All.UpdateNoteServer(note);
                }
            }
        }

        public void RemoveNote(long id)
        {
            if (id > 0)
            {
                using (var db = new dal.RTSEntities())
                {
                    dal.Note note = db.Notes.Find(id);
                    if (note != null)
                    {
                        db.Notes.Remove(note);
                        db.SaveChanges();
                    }
                    Clients.All.RemoveNoteServer(id);
                }
            }
        }
    }
}