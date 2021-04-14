# Torch API endpoint

## Base path

/api/v1/

Incremenet version as we make destructive changes to the API.

## General response text format

```
{
	"code": xxx,
	"message": "bababooey"
}
```

pretty standard.

## Authentication & Authorization

Simple pair of ID/password to authenticate & authorize via BA header.
Hit the permission API and see if the user has permission to the command.


```
/path/to/resource -H "Basic: <encoded id:password>"

400 Authentication failed
403 Insufficient permission
```

It'd be nice to have CRUD for user management in HTTP API but not necessary 
as long as we provide an equivalent command-line interface or config files.

## HTTPS

SSL must be the hard requirement for the bottom-line security whatever auth we may use.
We can provide HTTP access for users to risk it.

## Invoking a command

eg. invoking "!foo bar"

```
POST /command_asis?command=%21foo%20bar -H "Bearer: <access token>"

200 Command output
4xx client errors eg. syntax error, permission error, authentication error
5xx server errrors eg. runtime exception during command invocation
```
