from pathlib import Path
import re
root=Path(__file__).resolve().parents[1]
paths=['Models/User/Base/User.cs','Models/User/Classes/EmployeeUser.cs','Models/Business/Classes/Business.cs','Models/Business/Classes/Branch.cs']
for path in paths:
    server=(root/path).read_text(encoding='utf-8-sig')
    client=(root/'CoffeManager.Client.Admin'/path).read_text(encoding='utf-8-sig')
    block=lambda text:text.split('#region Structs',1)[1].split('#endregion Structs',1)[0]
    assert block(server)==block(client),path+' struct mismatch'
    for region in ['Operation Data','Task Account Operations','Task Profile Operations','Task Role Operations','Task Employee Operations']:
        if '#region '+region in client:assert '#region '+region in server,(path,region)
    print('PASS entity mirror:',path)
for p in root.rglob('Contracts/*.cs'):
    assert not re.search(r'\bstruct\b',p.read_text()),'Unexpected structure in '+str(p)
print('PASS Contracts contains no interaction structs')
