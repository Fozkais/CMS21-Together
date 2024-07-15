using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data.Vanilla
{
    [Serializable]
    public class ModNewJobPart
    {
        public string ID;
        public bool Done;
        public bool Found;

        public ModNewJobPart(NewJobPart part)
        {
            ID = part.ID;
            Done = part.Done;
            Found = part.Found;
        }

        public NewJobPart ToGame()
        {
            NewJobPart a = new NewJobPart();
            a.ID = this.ID;
            a.Done = this.Done;
            a.Found = this.Found;
            return a;
        }
    }
}