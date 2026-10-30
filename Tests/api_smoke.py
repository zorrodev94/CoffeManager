"""Local API smoke checks. Test-only TLS exception for a development certificate."""
import json, ssl, urllib.request, urllib.error
base='https://localhost:7066'
context=ssl._create_unverified_context()
def check(path,expected,body=None,headers=None):
    request=urllib.request.Request(base+path,data=None if body is None else json.dumps(body).encode(),headers={'Content-Type':'application/json',**(headers or {})})
    try:
        with urllib.request.urlopen(request,context=context,timeout=10) as response:code=response.status;payload=response.read()
    except urllib.error.HTTPError as response:code=response.code;payload=response.read()
    assert code in (expected if isinstance(expected,tuple) else (expected,)),(path,code,payload)
    print('PASS',path,code)
check('/api/health',200)
check('/api/users/me',401)
check('/api/employees',401)
check('/api/roles',401)
check('/api/accounts/available',401)
check('/api/catalogs/salary-bases',401)
check('/api/accounts/login',400,{}, {})
check('/api/accounts/login',400,{'email':'not-an-email','password':'x'},{'X-CoffeManager-Client':'Admin'})
check('/api/accounts/bootstrap',400,{'firstName':'x'*81,'paternalLastName':'Test','email':'x@example.com','password':'Test-Password-12345','setupKey':'x'*32},{'X-CoffeManager-Client':'Admin'})
check('/api/accounts/bootstrap',(401,503),{'firstName':'Test','paternalLastName':'Test','email':'x@example.com','password':'Test-Password-12345','setupKey':'x'*32},{'X-CoffeManager-Client':'Admin'})
print('No accounts or business data were created; SQL was not required by these checks.')
