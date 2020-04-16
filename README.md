# REST_MySQL

## Tutorial

<>

## generate controller

```
dotnet aspnet-codegenerator controller -name PeopleController -async -api -m Person -dc PersonContext -outDir Controllers
```

## API details

```
Method name	API call
getList	GET http://my.api.url/posts?sort=["title","ASC"]&range=[0, 24]&filter={"title":"bar"}
getOne	GET http://my.api.url/posts/123
getMany	GET http://my.api.url/posts?filter={"id":[123,456,789]}
getManyReference	GET http://my.api.url/posts?filter={"author_id":345}
create	POST http://my.api.url/posts/123
update	PUT http://my.api.url/posts/123
updateMany	Multiple calls to PUT http://my.api.url/posts/123
delete	DELETE http://my.api.url/posts/123
deteleMany	Multiple calls to DELETE http://my.api.url/posts/123
```
