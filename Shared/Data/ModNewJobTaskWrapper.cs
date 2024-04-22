using System;
using System.Collections.Generic;
using Il2Cpp;

namespace CMS21Together.Shared.Data
{
    [Serializable]
    public class ModNewJobTaskWrapper
    {
        public string type;
        public string subtype;
        public int IncreaseTuneValue;
        public int partsCount;
        public string desc = "";
        public bool easyMode;
        public int moneySpent;
        public List<ModNewJobPart> Parts;
        public bool _done;

        public ModNewJobTaskWrapper(NewJobTaskWrapper data)
        {
            type = data.type;
            subtype = data.subtype;
            IncreaseTuneValue = data.IncreaseTuneValue;
            partsCount = data.partsCount;
            desc = data.desc;
            easyMode = data.easyMode;
            moneySpent = data.moneySpent;
           Parts = new List<ModNewJobPart>();
    
            // Copy parts
            if (this.Parts != null)
            {
                foreach (NewJobPart part in data.Parts)
                {
                    Parts.Add(new ModNewJobPart(part));
                }
            }

            _done = data._done;
        }

        public NewJobTaskWrapper ToGame()
        {
            NewJobTaskWrapper jobTaskWrapper = new NewJobTaskWrapper();

            jobTaskWrapper.type = this.type;
            jobTaskWrapper.subtype = this.subtype;
            jobTaskWrapper.IncreaseTuneValue = this.IncreaseTuneValue;
            jobTaskWrapper.partsCount = this.partsCount;
            jobTaskWrapper.desc = this.desc;
            jobTaskWrapper.easyMode = this.easyMode;
            jobTaskWrapper.moneySpent = this.moneySpent;
            jobTaskWrapper.Parts = new Il2CppSystem.Collections.Generic.List<NewJobPart>();
    
            // Copy parts
            if (this.Parts != null)
            {
                foreach (ModNewJobPart part in this.Parts)
                {
                    jobTaskWrapper.Parts.Add(part.ToGame());
                }
            }

            jobTaskWrapper._done = this._done;

            return jobTaskWrapper;
        }
		
    }
}