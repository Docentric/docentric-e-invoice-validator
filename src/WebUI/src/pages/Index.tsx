import { useState } from "react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useToast } from "@/hooks/use-toast";
import { Upload, CheckCircle, XCircle, FileText, AlertCircle, Download, Beaker } from "lucide-react";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";

interface ValidationResult {
  exitCode: number;
  status: "valid" | "invalid" | "unknown";
  xml: string;
  stderr: string;
}

/** Health check result interface */
interface ServerHealthResult {
  /** Indicates if the server is healthy */
  healthy: boolean;
  /** Is Java present on the server */
  javaPresent: boolean;
  /** The version of Java installed on the server */
  javaVersion: string | null;
  /** Is Mustang CLI present on the server */
  mustangCliPresent: boolean;
  /** The version of Mustang CLI installed on the server */
  mustangCliVersion: string | null;
}

/** Base interface for operation results */
interface BaseFileOperationResult {
  /** Indicates if the operation was successful */
  success: boolean;
  /** Error code */
  errorCode: string;
  /** Error message */
  errorMessage: string | null;
  /** Diagnostics error message containing details about the error */
  diagnosticsErrorMessage: string | null;
}

/* General file validation result interface */
interface FileValidationResult extends BaseFileOperationResult {
  /* Indicates if the file is valid */
  isValid: boolean;
  /* XML validation report as string */
  validationReport: string;
}

/* PDF file validation result interface */
interface PdfFileValidationResult extends BaseFileOperationResult {
  /* Indicates if the file is valid */
  isValid: boolean;
  /* Indicates if the PDF signature is valid */
  isSignatureValid: boolean;
  /* XML validation report as string */
  validationReport: string;
}

/* XML file validation result interface */
interface ExtractXmlFromPdfResult extends BaseFileOperationResult {
  /* Extracted XML content as string */
  xml: string | null;
}

/* Convert XML to PDF result interface */
interface ConvertXmlToPdfResult extends BaseFileOperationResult {
  /* Generated PDF content as base64 string */
  pdf: string | null;
}

type OperationType = "validate-pdf" | "validate-xml" | "extract" | "convert-xml-to-pdf";

// Mapping: operation → result type
type OperationResultMap = {
  "validate-pdf": PdfFileValidationResult;
  "validate-xml": FileValidationResult;
  "extract": ExtractXmlFromPdfResult;
  "convert-xml-to-pdf": ConvertXmlToPdfResult;
};

const Index = () => {
  const simulationModeEnabled: boolean = false;
  const [file, setFile] = useState<File | null>(null);
  const [isDragging, setIsDragging] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [operation, setOperation] = useState<OperationType>("validate-pdf");
  const [result, setResult] = useState<BaseFileOperationResult | null>(null);
  const [simulationMode, setSimulationMode] = useState(false);
  const { toast } = useToast();

  const getUrlInfo = () => {
    const url = new URL(window.location.href);

    return {
      currentUrl: url.href,
      siteUrl: url.origin,
      virtualPath: url.pathname,
      queryParams: Object.fromEntries(url.searchParams.entries())
    };
  };

  // type OperationType = "validate-pdf" | "validate-xml" | "extract" | "convert-xml-to-pdf";
  const getApiEndPoint = (operation) => {
    const apiEndpoints = {
      "validate-pdf": "api/pdf/validate-zugferd",
      "validate-xml": "api/xml/validate",
      "extract": "api/pdf/extract-zugferd-xml",
      "convert-xml-to-pdf": "api/xml/convert-to-pdf"
    };

    return apiEndpoints[operation];
  };

  // Mock response data for simulation
  const getMockResponse = <TOperation extends OperationType>(operation: TOperation): OperationResultMap[TOperation] => {
    const mockResponses = {
      "validate-pdf": {
        success: true,
        errorCode: "Success",
        errorMessage: null,
        diagnosticsErrorMessage: null,
        isValid: true,
        isSignatureValid: true,
        validationReport:`<?xml version="1.0" encoding="UTF-8"?>
<validation filename="mock-invoice.pdf" datetime="2024-01-15 10:30:00">
  <pdf>
    <info>
      <pdfAVersion>PDF/A-3</pdfAVersion>
      <pdfVersion>1.7</pdfVersion>
    </info>
    <summary status="valid"/>
  </pdf>
  <xml>
    <info>
      <version>2</version>
      <profile>urn:cen.eu:en16931:2017#compliant:factur-x.eu:1p0:en16931</profile>
      <validator version="2.20.0"/>
      <rules>
        <fired>201</fired>
        <failed>0</failed>
      </rules>
      <duration unit="ms">1250</duration>
    </info>
    <summary status="valid"/>
  </xml>
  <summary status="valid"/>
</validation>`
      },
      "validate-xml": {
        success: true,
        errorCode: "Success",
        errorMessage: null,
        diagnosticsErrorMessage: null,
        isValid: true,
        validationReport: `<?xml version="1.0" encoding="UTF-8"?>
<validation filename="mock-factur-x.xml" datetime="2024-01-15 10:32:00">
  <xml>
    <info>
      <version>2</version>
      <profile>urn:cen.eu:en16931:2017#compliant:factur-x.eu:1p0:en16931</profile>
      <validator version="2.20.0"/>
      <rules>
        <fired>195</fired>
        <failed>0</failed>
      </rules>
      <duration unit="ms">950</duration>
    </info>
    <summary status="valid"/>
  </xml>
  <summary status="valid"/>
</validation>`
      },
      "extract": {
        success: true,
        errorCode: "Success",
        errorMessage: null,
        diagnosticsErrorMessage: null,
        xml: `<?xml version="1.0" encoding="UTF-8"?>
<rsm:CrossIndustryInvoice xmlns:rsm="urn:un:unece:uncefact:data:standard:CrossIndustryInvoice:100">
  <rsm:ExchangedDocumentContext>
    <ram:GuidelineSpecifiedDocumentContextParameter>
      <ram:ID>urn:cen.eu:en16931:2017#compliant:factur-x.eu:1p0:en16931</ram:ID>
    </ram:GuidelineSpecifiedDocumentContextParameter>
  </rsm:ExchangedDocumentContext>
  <rsm:ExchangedDocument>
    <ram:ID>MOCK-INV-2024-001</ram:ID>
    <ram:TypeCode>380</ram:TypeCode>
    <ram:IssueDateTime>
      <udt:DateTimeString format="102">20240115</udt:DateTimeString>
    </ram:IssueDateTime>
  </rsm:ExchangedDocument>
  <rsm:SupplyChainTradeTransaction>
    <ram:ApplicableHeaderTradeSettlement>
      <ram:SpecifiedTradeSettlementHeaderMonetarySummation>
        <ram:TaxBasisTotalAmount>1000.00</ram:TaxBasisTotalAmount>
        <ram:TaxTotalAmount currencyID="EUR">190.00</ram:TaxTotalAmount>
        <ram:GrandTotalAmount>1190.00</ram:GrandTotalAmount>
      </ram:SpecifiedTradeSettlementHeaderMonetarySummation>
    </ram:ApplicableHeaderTradeSettlement>
  </rsm:SupplyChainTradeTransaction>
</rsm:CrossIndustryInvoice>`
      },
      "convert-xml-to-pdf": {
        success: true,
        errorCode: "Success",
        errorMessage: null,
        diagnosticsErrorMessage: null,
        pdf: "JVBERi0xLjQKJeLjz9MKNCAwIG9iago8PC9UeXBlIC9QYWdlCi9QYXJlbnQgMyAwIFIKL01lZGlhQm94IFswIDAgNTk1IDg0Ml0KL0NvbnRlbnRzIDUgMCBSCj4+CmVuZG9iago1IDAgb2JqCjw8L0xlbmd0aCA0Nj4+CnN0cmVhbQpCVAovRjEgMjQgVGYKMTAwIDcwMCBUZAooTW9jayBJbnZvaWNlIFBERikgVGoKRVQKZW5kc3RyZWFtCmVuZG9iagozIDAgb2JqCjw8L1R5cGUgL1BhZ2VzCi9LaWRzIFs0IDAgUl0KL0NvdW50IDEKL1Jlc291cmNlcyA8PC9Gb250IDw8L0YxIDYgMCBSPj4+Pgo+PgplbmRvYmoKNiAwIG9iago8PC9UeXBlIC9Gb250Ci9TdWJ0eXBlIC9UeXBlMQovQmFzZUZvbnQgL0hlbHZldGljYQo+PgplbmRvYmoKMSAwIG9iago8PC9UeXBlIC9DYXRhbG9nCi9QYWdlcyAzIDAgUgo+PgplbmRvYmoKMiAwIG9iago8PC9Qcm9kdWNlciAoTW9jayBQREYgR2VuZXJhdG9yKQo+PgplbmRvYmoKeHJlZgowIDcKMDAwMDAwMDAwMCA2NTUzNSBmDQowMDAwMDAwMzY4IDAwMDAwIG4NCjAwMDAwMDA0MTcgMDAwMDAgbg0KMDAwMDAwMDIyOCAwMDAwMCBuDQowMDAwMDAwMDE1IDAwMDAwIG4NCjAwMDAwMDAxMDkgMDAwMDAgbg0KMDAwMDAwMDMxNyAwMDAwMCBuDQp0cmFpbGVyCjw8L1NpemUgNwovUm9vdCAxIDAgUgovSW5mbyAyIDAgUgo+PgpzdGFydHhyZWYKNDY1CiUlRU9G"
      }
    } satisfies OperationResultMap;

    return mockResponses[operation];
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = () => {
    setIsDragging(false);
  };

  const handleDownloadPDF = () => {
    const pdfContentBase64 = (result as ConvertXmlToPdfResult)?.pdf;

    if (!pdfContentBase64) return;

    const byteCharacters = atob(pdfContentBase64);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: 'application/pdf' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `converted-e-invoice-${Date.now()}.pdf`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
    toast({
      title: "Downloaded",
      description: "PDF file has been downloaded successfully",
      variant: "success"
    });
  };

  const handleDownloadXML = () => {
    const xmlContent = (result as FileValidationResult | PdfFileValidationResult)?.validationReport || (result as ExtractXmlFromPdfResult)?.xml;

    if (!xmlContent) return;

    console.log(operation);

    var filename: string = `Validation-Report-${Date.now()}.xml`;
    if ((result as ExtractXmlFromPdfResult)?.xml) {
      filename = `Extracted-e-Invoice-attachment-${Date.now()}.xml`;
    }

    const blob = new Blob([xmlContent], { type: 'application/xml' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);

    toast({
      title: "Downloaded",
      description: "XML file has been downloaded successfully",
      variant: "success"
    });
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
    const droppedFile = e.dataTransfer.files[0];
    if (droppedFile) {
      setFile(droppedFile);
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      setFile(e.target.files[0]);
    }
  };

  const handleOperation = async () => {
    // Simulation mode - return mock data immediately
    if (simulationMode) {
      setIsLoading(true);
      setResult(null);
      const startTime = performance.now();

      // Auto-create mock file if none selected
      if (!file) {
        const mockFileName = operation === "validate-xml" || operation === "convert-xml-to-pdf" ? "mock-invoice.xml" : "mock-invoice.pdf";
        const mockType = operation === "validate-xml" ? "application/xml" : "application/pdf";
        const mockContent = operation === "validate-xml"
          ? '<?xml version="1.0"?><Invoice></Invoice>'
          : "%PDF-1.4 mock content";
        
        const mockFile = new File([mockContent], mockFileName, { type: mockType });
        setFile(mockFile);
      }

      // Simulate API delay
      setTimeout(() => {
        const mockData = getMockResponse(operation);
        setResult(mockData);
        setIsLoading(false);
        const duration = Math.round(performance.now() - startTime);

        toast({
          title: "🧪 Simulation Complete",
          description: `Mock ${operation} result generated in ${duration}ms`,
        });
      }, 800);
      return;
    }

    // Real mode - validate and make API call
    if (!file) {
      toast({
        title: "No file selected",
        description: "Please select a PDF or XML file",
        variant: "destructive",
      });
      return;
    }

    // Validate file type based on operation
    const isXML = file.name.toLowerCase().endsWith('.xml');
    const isPDF = file.name.toLowerCase().endsWith('.pdf');
    
    if (operation === "validate-xml" && !isXML) {
      toast({
        title: "Invalid file type",
        description: "Please select an XML file for XML validation",
        variant: "destructive",
      });
      return;
    }
    
    if ((operation === "validate-pdf" || operation === "extract") && !isPDF) {
      toast({
        title: "Invalid file type",
        description: "Please select a PDF file for this operation",
        variant: "destructive",
      });
      return;
    }

    setIsLoading(true);
    setResult(null);
    const startTime = performance.now();

    try {
      const formData = new FormData();
      formData.append("file", file);

      const response = await fetch(`${getUrlInfo().siteUrl}/${getApiEndPoint(operation)}`, {
        method: "POST",
        body: formData,
      });

      const data = await response.json() as OperationResultMap[typeof operation];
      console.log(data);
      setResult(data);
      const duration = Math.round(performance.now() - startTime);


      const operationLabels: Record<OperationType, string> = {
        "validate-pdf": "PDF Validation",
        "validate-xml": "XML Validation",
        "extract": "XML Extraction",
        "convert-xml-to-pdf": "XML to PDF Conversion"
      };

      const toastTitle: string = `${operationLabels[operation]}`;
      type toastVariantType = "default" | "success" | "info" | "warning" | "destructive";
      var toastVariant: toastVariantType = data.success ? "success" : "destructive";

      if (operation === "validate-pdf" || operation === "validate-xml") {
        if ((data as PdfFileValidationResult | FileValidationResult)?.isValid === false) {
          toastVariant = "warning";
        }
      }

      const getDescription = () => {
        const fileSize = (file.size / 1024).toFixed(1);
        const timeStr = duration < 1000 ? `${duration}ms` : `${(duration / 1000).toFixed(2)}s`;

        switch (operation) {
          case "extract":
            return data.errorCode === "Success"
              ? `XML extracted successfully from ${file.name} (${fileSize} KB) in ${timeStr}`
              : `XML extraction failed after ${timeStr}. ${data.errorMessage || `Error code: ${data.errorCode}`}`;
          case "convert-xml-to-pdf":
            return data.errorCode === "Success"
              ? `PDF generated successfully from ${file.name} (${fileSize} KB) in ${timeStr}`
              : `PDF generation failed after ${timeStr}. ${data.errorMessage || `Error code: ${data.errorCode}`}`;
          case "validate-pdf":
            const pdfResult = data as PdfFileValidationResult;
            return pdfResult.isValid
              ? `${file.name} (${fileSize} KB) is valid. Processed in ${timeStr}`
              : `${file.name} (${fileSize} KB) validation failed. Processed in ${timeStr}`;
          case "validate-xml":
            const xmlResult = data as FileValidationResult;
            return xmlResult.isValid
              ? `${file.name} (${fileSize} KB) is valid. Processed in ${timeStr}`
              : `${file.name} (${fileSize} KB) validation failed. Processed in ${timeStr}`;
        }
      };

      toast({
        title: toastTitle,
        description: getDescription(),
        variant: toastVariant,
      });
    } catch (error) {
      const duration = Math.round(performance.now() - startTime);
      const timeStr = duration < 1000 ? `${duration}ms` : `${(duration / 1000).toFixed(2)}s`;

      toast({
        title: "Operation Failed",
        description: `${error instanceof Error ? error.message : "Operation failed"} (after ${timeStr})`,
        variant: "destructive",
      });
    } finally {
      setIsLoading(false);
    }
  };

  const getEscapedString = (input: string) => {
    return input?.replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;") ?? "";
  }
    
  const getResultFile = (source: BaseFileOperationResult) => {
    let result = (source as FileValidationResult | PdfFileValidationResult)?.validationReport ||
      (source as ExtractXmlFromPdfResult)?.xml;

    if (!result) {
      result = (source as ConvertXmlToPdfResult)?.pdf;
    }

    return result;
  }

  return (
    <div className="min-h-screen bg-gradient-to-b from-background to-secondary">
      {/* Header */}
      <header className="border-b border-border bg-card/50 backdrop-blur-sm">
        <div className="container mx-auto px-4 py-6">
          <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent">
            Docentric ZuGFeRD, Factur-X and UBL Document Validator
          </h1>
          <p className="mt-2 text-muted-foreground">
            A lightweight REST API wrapper around the <a href="https://github.com/Docentric/Mustang-CLI" target="_blank" rel="noopener noreferrer">Mustang-Project CLI</a> for validating ZuGFeRD, Factur-X and UBL documents. Fast, reliable, and easy to integrate.
          </p>
          <div className="mt-4 grid grid-cols-1 md:grid-cols-2 gap-3 text-sm">
            <div className="flex items-center gap-2">
              <CheckCircle className="h-4 w-4 text-success" />
              <span className="font-semibold">Java:</span>
              <span className="text-muted-foreground">Available (OpenJDK 17.0.2)</span>
            </div>
            <div className="flex items-center gap-2">
              <CheckCircle className="h-4 w-4 text-success" />
              <span className="font-semibold">Mustang CLI:</span>
              <span className="text-muted-foreground">Available (v2.20.0)</span>
            </div>
          </div>
        </div>
      </header>

      <main className="container mx-auto px-4 py-8">
        {/* Interactive Tester Section */}
        <section className="mb-12">
          <Card className="overflow-hidden border-2 border-primary/20 shadow-lg">
            <div className="bg-gradient-to-r from-primary to-accent p-6">
              <div className="flex items-start justify-between">
                <div>
                  <h2 className="text-2xl font-bold text-primary-foreground">Test your ZuGFeRD, Factur-X or UBL e-Invoices</h2>
                  <p className="mt-1 text-primary-foreground/90">
                    Upload, validate, extract or convert files
                  </p>
                </div>
                { simulationModeEnabled && (
                  <div className="flex items-center gap-3 bg-background/10 backdrop-blur-sm rounded-lg px-4 py-2">
                    <Beaker className="h-4 w-4 text-primary-foreground" />
                    <Label htmlFor="simulation-mode" className="text-sm font-medium text-primary-foreground cursor-pointer">
                      Simulation Mode
                    </Label>
                    <Switch
                      id="simulation-mode"
                      checked={simulationMode}
                      onCheckedChange={setSimulationMode}
                    />
                  </div>
                )}
              </div>
            </div>
            
            <div className="p-6 space-y-6">
              {/* File Upload Area */}
              <div
                onDragOver={handleDragOver}
                onDragLeave={handleDragLeave}
                onDrop={handleDrop}
                className={`relative border-2 border-dashed rounded-lg p-8 text-center transition-all ${
                  isDragging
                    ? "border-primary bg-primary/5 scale-105"
                    : "border-border hover:border-primary/50"
                }`}
              >
                <input
                  type="file"
                  id="file-upload"
                  className="hidden"
                  accept=".pdf,.xml"
                  onChange={handleFileChange}
                />
                <label htmlFor="file-upload" className="cursor-pointer">
                  <Upload className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
                  <p className="text-lg font-medium text-foreground mb-2">
                    {file ? file.name : "Drop your file here or click to browse"}
                  </p>
                  <p className="text-sm text-muted-foreground">
                    Supports PDF and XML files
                  </p>
                </label>
              </div>

              {/* Operation Selection */}
              <div className="space-y-3">
                <label className="text-sm font-semibold text-foreground">Select Operation</label>
                <div className="grid grid-cols-1 md:grid-cols-4 gap-3">
                  <Button
                    variant={operation === "validate-pdf" ? "default" : "outline"}
                    onClick={() => { setResult(null); setOperation("validate-pdf"); }}
                    className="h-auto py-4 flex flex-col items-start gap-2"
                  >
                    <span className="font-semibold">Validate PDF</span>
                    <span className="text-xs opacity-80">ZuGFeRD PDF validation</span>
                  </Button>
                  <Button
                    variant={operation === "validate-xml" ? "default" : "outline"}
                    onClick={() => { setResult(null); setOperation("validate-xml"); }}
                    className="h-auto py-4 flex flex-col items-start gap-2"
                  >
                    <span className="font-semibold">Validate XML</span>
                    <span className="text-xs opacity-80">Factur-X or UBL XML validation</span>
                  </Button>
                  <Button
                    variant={operation === "extract" ? "default" : "outline"}
                    onClick={() => { setResult(null); setOperation("extract"); }}
                    className="h-auto py-4 flex flex-col items-start gap-2"
                  >
                    <span className="font-semibold">Extract XML from PDF</span>
                    <span className="text-xs opacity-80">Extract XML from ZuGFeRD PDF</span>
                  </Button>
                  <Button
                    variant={operation === "convert-xml-to-pdf" ? "default" : "outline"}
                    onClick={() => { setResult(null); setOperation("convert-xml-to-pdf"); } }
                    className="h-auto py-4 flex flex-col items-start gap-2"
                  >
                    <span className="font-semibold">Convert XML to PDF</span>
                    <span className="text-xs opacity-80">Convert Factur-X or UBL to PDF</span>
                  </Button>
                </div>
              </div>

              <Button
                onClick={handleOperation}
                disabled={!file || isLoading}
                className="w-full bg-gradient-to-r from-primary to-accent hover:opacity-90 transition-opacity"
                size="lg"
              >
                {isLoading ? "Processing..." : (
                  operation === "extract" ? "Extract XML from PDF" : 
                  operation === "validate-xml" ? "Validate XML" : 
                  operation === "convert-xml-to-pdf" ? "Convert XML to PDF" :
                  "Validate PDF"
                )}
              </Button>

              {/* Results */}
              {result && (
                <Card className="border-2 p-6 space-y-4">
                  <div className="flex items-center gap-3">
                    {result.success === true ? (
                      <CheckCircle className="h-8 w-8 text-success" />
                    ) : (
                        /*<XCircle className="h-8 w-8 text-destructive" />*/
                        <AlertCircle className="h-8 w-8 text-muted-foreground" />
                    )}
                    <div>
                      <h3 className="text-xl font-bold capitalize">{result.success ? "valid" : "invalid"}</h3>
                      {/*<p className="text-sm text-muted-foreground">*/}
                      {/*  Exit code: {result.errorCode}*/}
                      {/*  {result.errorCode !== 0 && ` - ${result.errorMessage}`}*/}
                      {/*</p>*/}
                    </div>
                  </div>

                  {((result as FileValidationResult | PdfFileValidationResult)?.validationReport !== undefined || (result as ExtractXmlFromPdfResult)?.xml !== undefined || (result as ConvertXmlToPdfResult)?.pdf) !== undefined && (
                    <div>
                      <div className="flex items-center justify-between mb-2">
                        <h4 className="font-semibold flex items-center gap-2">
                          <FileText className="h-4 w-4" />
                          {operation === "extract" ? "Extracted XML" : operation === "convert-xml-to-pdf" ? "Generated PDF document in base64 encoding" : "Validation report"}
                        </h4>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={operation === "convert-xml-to-pdf" ? handleDownloadPDF : handleDownloadXML}
                          className="gap-2"
                        >
                          <Download className="h-4 w-4" />
                          {operation === "extract" ? "Download e-Invoice XML" : operation === "convert-xml-to-pdf" ? "Download Invoice PDF" : "Download validation report XML"}
                        </Button>
                      </div>
                      <pre className="bg-code-bg text-code-foreground p-4 rounded-lg overflow-x-auto text-xs">
                        {getResultFile(result)}
                      </pre>
                    </div>
                  )}

                  {result.diagnosticsErrorMessage && (
                    <div>
                      <h4 className="font-semibold mb-2">Diagnostics</h4>
                      <pre className="bg-muted p-4 rounded-lg overflow-x-auto text-xs">
                        {getEscapedString(result.diagnosticsErrorMessage)}
                      </pre>
                    </div>
                  )}
                </Card>
              )}
            </div>
          </Card>
        </section>

        {/* Sample Files Section */}
        <section className="mb-12">
          <h2 className="text-2xl font-bold mb-6">Sample Files</h2>
          <Card className="p-6 bg-card/50 backdrop-blur border-border/50">
            <div className="flex items-start gap-4">
              <div className="flex-shrink-0">
                <FileText className="h-8 w-8 text-primary" />
              </div>
              <div className="flex-1">
                <h3 className="font-semibold text-lg mb-2">ZUGFeRD Sample Corpus</h3>
                <p className="text-muted-foreground mb-4">
                  Access a comprehensive collection of sample files including ZUGFeRD, Factur-X, and UBL formats to test the validator with real-world examples.
                </p>
                <Button variant="outline" asChild>
                  <a href="https://github.com/ZUGFeRD/corpus" target="_blank" rel="noopener noreferrer" className="inline-flex items-center gap-2">
                    <Download className="h-4 w-4" />
                    View Sample Files on GitHub
                  </a>
                </Button>
              </div>
            </div>
          </Card>
        </section>

        {/* API Documentation */}
        <section className="space-y-6">
          <h2 className="text-2xl font-bold">API Documentation</h2>

          <Card className="overflow-hidden">
            <div className="bg-gradient-to-r from-primary to-accent p-6">
              <h3 className="text-xl font-bold text-primary-foreground">Interactive API Documentation</h3>
              <p className="mt-2 text-primary-foreground/90">
                Explore and test all API endpoints with our interactive documentation
              </p>
            </div>
            <div className="p-6 space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <a
                  href="api/docs/swagger"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="block"
                >
                  <Card className="p-6 hover:border-primary transition-colors cursor-pointer h-full">
                    <div className="flex items-start gap-4">
                      <div className="p-3 rounded-lg bg-primary/10">
                        <FileText className="h-6 w-6 text-primary" />
                      </div>
                      <div className="flex-1">
                        <h4 className="font-semibold text-lg mb-1">Swagger UI</h4>
                        <p className="text-sm text-muted-foreground mb-3">
                          Interactive API explorer with request/response examples
                        </p>
                        <span className="text-xs font-mono text-primary">{getUrlInfo().siteUrl}/api/docs/swagger →</span>
                      </div>
                    </div>
                  </Card>
                </a>

                <a
                  href="api/docs/redoc"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="block"
                >
                  <Card className="p-6 hover:border-primary transition-colors cursor-pointer h-full">
                    <div className="flex items-start gap-4">
                      <div className="p-3 rounded-lg bg-accent/10">
                        <FileText className="h-6 w-6 text-accent" />
                      </div>
                      <div className="flex-1">
                        <h4 className="font-semibold text-lg mb-1">ReDoc</h4>
                        <p className="text-sm text-muted-foreground mb-3">
                          Clean, responsive API documentation viewer
                        </p>
                        <span className="text-xs font-mono text-accent">{getUrlInfo().siteUrl}/api/docs/redoc →</span>
                      </div>
                    </div>
                  </Card>
                </a>
              </div>
            </div>
          </Card>
        </section>
      </main>

      {/* Licensing Section */}
      <section className="mt-12 mb-12">
        <div className="container mx-auto px-4 space-y-6">
          <h2 className="text-2xl font-bold">Licensing Information</h2>

          <Card className="overflow-hidden">
            <div className="bg-gradient-to-r from-primary to-accent p-6">
              <h3 className="text-xl font-bold text-primary-foreground">Open Source Licenses</h3>
              <p className="mt-2 text-primary-foreground/90">
                This project and its dependencies are licensed under open source licenses
              </p>
            </div>

            <div className="p-6 space-y-6">
              {/* Project License */}
              <div>
                <h4 className="text-lg font-semibold mb-3">Project License (MIT)</h4>
                <div className="bg-muted p-4 rounded-lg text-sm space-y-2">
                  <p className="font-mono text-xs">MIT License</p>
                  <p className="font-mono text-xs">Copyright (c) 2025 Docentric</p>
                  <p className="text-muted-foreground mt-3">
                    Permission is hereby granted, free of charge, to any person obtaining a copy
                    of this software and associated documentation files (the "Software"), to deal
                    in the Software without restriction, including without limitation the rights
                    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
                    copies of the Software, and to permit persons to whom the Software is
                    furnished to do so, subject to the following conditions:
                  </p>
                  <p className="text-muted-foreground mt-2">
                    The above copyright notice and this permission notice shall be included in
                    all copies or substantial portions of the Software.
                  </p>
                  <p className="text-muted-foreground mt-2">
                    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
                    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
                    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
                    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
                    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
                    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
                    THE SOFTWARE.
                  </p>
                </div>
              </div>

              {/* Third-Party License */}
              <div>
                <h4 className="text-lg font-semibold mb-3">Third-Party Components</h4>
                <Card className="border-2">
                  <div className="p-4">
                    <div className="flex items-start gap-4">
                      <div className="p-3 rounded-lg bg-primary/10">
                        <FileText className="h-6 w-6 text-primary" />
                      </div>
                      <div className="flex-1">
                        <h5 className="font-semibold text-lg mb-2">Mustang project / Mustang-CLI</h5>
                        <p className="text-sm text-muted-foreground mb-3">
                          This project integrates with the Mustang project CLI for ZUGFeRD/Factur-X/UBL
                          validation and PDF/A processing.
                        </p>
                        <div className="space-y-1 text-sm">
                          <p>
                            <span className="font-semibold">Project:</span>{" "}
                            <a href="https://mustangproject.org/" target="_blank" rel="noopener noreferrer" className="text-primary hover:underline">
                              https://mustangproject.org/
                            </a>
                          </p>
                          <p>
                            <span className="font-semibold">Mustang-CLI:</span>{" "}
                            <a href="https://www.mustangproject.org/commandline/" target="_blank" rel="noopener noreferrer" className="text-primary hover:underline">
                              https://www.mustangproject.org/commandline/
                            </a>
                          </p>
                          <p>
                            <span className="font-semibold">Repository:</span>{" "}
                            <a href="https://github.com/ZUGFeRD/mustangproject" target="_blank" rel="noopener noreferrer" className="text-primary hover:underline">
                              https://github.com/ZUGFeRD/mustangproject
                            </a>
                          </p>
                          <p>
                            <span className="font-semibold">License:</span> Apache License 2.0
                          </p>
                          <p className="text-muted-foreground mt-2">
                            Copyright © 2013-2025 ZUGFeRD Association e.V.
                          </p>
                        </div>
                      </div>
                    </div>
                  </div>
                </Card>
              </div>

              {/* Acknowledgements */}
              <div>
                <h4 className="text-lg font-semibold mb-3">Acknowledgements</h4>
                <div className="bg-muted p-4 rounded-lg text-sm text-muted-foreground">
                  <p>
                    We gratefully acknowledge the Mustang project team and the ZUGFeRD Association
                    for their excellent work in creating and maintaining the Mustang-CLI tool,
                    which enables validation, extraction, and processing of ZUGFeRD/UBL/Factur-X documents.
                  </p>
                  <p className="mt-2">
                    If you bundle the Mustang-CLI JAR or Docker image with this project, the full
                    Apache License 2.0 text should be included in your distribution.
                  </p>
                </div>
              </div>

              {/* Kudos to Lovable */}
              <div>
                <h4 className="text-lg font-semibold mb-3">Kudos to Lovable</h4>
                <div className="bg-muted p-4 rounded-lg text-sm text-muted-foreground">
                  <p>
                    Special thanks to the <a href="https://lovable.dev/" target="_blank" rel="noopener noreferrer" className="text-primary hover:underline">Lovable platform</a> for accelerating development, improving code quality, and making the overall project experience smoother and more enjoyable.
                  </p>
                </div>
              </div>
            </div>
          </Card>
        </div>
      </section>

      {/* Footer */}
      <footer className="mt-12 border-t border-border bg-card/50 backdrop-blur-sm">
        <div className="container mx-auto px-4 py-6 text-center text-sm text-muted-foreground">
          <p>Docentric ZuGFeRD, Factur-X and UBL Document Validator</p>
        </div>
      </footer>
    </div>
  );
};

export default Index;
