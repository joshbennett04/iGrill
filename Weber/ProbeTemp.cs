namespace Weber
{
    public class ProbeTemp
    {
        private string m_Name = string.Empty;
        private string m_Temp = string.Empty;
        private bool m_PendingRemoval = false;

        public ProbeTemp()
        {
        }

        public ProbeTemp(string probeName, string probeTemp, bool pendingRemoval)
        {
            m_Name = probeName;
            m_Temp = probeTemp;
        }

        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
            }
        }

        public string Temp
        {
            get
            {
                return m_Temp;
            }
            set
            {
                m_Temp = value;
            }
        }

        public bool PendingRemoval
        {
            get
            {
                return m_PendingRemoval;
            }
            set
            {
                m_PendingRemoval = value;
            }
        }
    }
}
