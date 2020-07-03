# REST_MySQL

## API details

```
Method name	API call
getList	GET http://my.api.url/api/people/?sort=["name","ASC"]&range=[0, 24]&filter={"name":"m"}
getOne	GET http://my.api.url/api/people/123
getMany	GET http://my.api.url/api/people/?filter={"uid":[2,4,3]}
getManyReference	GET http://my.api.url/api/people?filter={"name":"Tom"}
create	POST http://my.api.url/api/people/
update	PUT http://my.api.url/api/people/123
updateMany	Multiple calls to PUT http://my.api.url/api/people/123
delete	DELETE http://my.api.url/api/people/123
deteleMany	Multiple calls to DELETE http://my.api.url/api/people/123
```

When creating, put new data to the body in json format.

## Tutorial

<https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-3.1&tabs=visual-studio-code>

## generate controller

```
dotnet aspnet-codegenerator controller -name PeopleController -async -api -m Person -dc PersonContext -outDir Controllers
```

## Credit

- tYoshiyuki's [dotnet-core-react-admin](https://github.com/tYoshiyuki/dotnet-core-react-admin)

## TODO

- [ ] Enable `getMany`
