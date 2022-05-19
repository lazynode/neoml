# neoml

lazy language and compiler for neo-vm

## linting & validation

### prerequest

XSD v1.0 is used to support linting and validation. Use any editor with a XML plugin that support XSD will be OK. We recommend VSCode and redhat's XML plugin.

### usage

We provide different levels of language in different namespace. Use [Assembly.xsd](./Assembly.xsd) for Assembly linting and validation.

Below are some different ways for using this xsd. Choose any way you like.

#### xml-model with XSD

Add xml-model at the begining of all scripts.

```xml
<?xml-model href="https://lazynode.github.io/neoml/Assembly.xsd"?>
<lazy xmlns="Assembly" >
    <literal type="int" val="2" />
    <literal type="int" val="3" />
    <instruction opcode="add"/>
</lazy>
```

#### use schemaLocation

Add schemaLocation at the root node of all scripts.

```xml
<lazy xmlns="Assembly" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
xsi:schemaLocation="Assembly https://lazynode.github.io/neoml/Assembly.xsd">
    <literal type="int" val="2" />
    <literal type="int" val="3" />
    <instruction opcode="add" />
    <nop />
    <contractcall hash=""></contractcall>
</lazy>
```

#### XML catalog with XSD

**VSCode Only** Bind namespace `Assembly` to our XSD file globally if you don't want to set the real path of the XSD file for each XML document. Put such a `catalog.xml`'s absolute path as a value to vscode's settings `xml.catalogs`.

```xml
<catalog xmlns="urn:oasis:names:tc:entity:xmlns:xml:catalog">
  <uri
      name="Assembly"
      uri="https://lazynode.github.io/neoml/Assembly.xsd" />
</catalog>
```

#### XML file association with XSD

**VSCode Only** You can use the XML file association strategy to bind the XML file `assembly.xml` with the XSD file `Assembly.xsd` by adding the following into your vscode settings.json:

```json
"xml.fileAssociations": [
   {
       "pattern": "foo*.xml",
       "systemId": "Assembly.xsd"
   }
]
```
