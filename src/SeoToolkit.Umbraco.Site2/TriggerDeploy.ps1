<#
    .DESCRIPTION
        This will send a request to trigger a Deploy extraction or return a valid bearer header token
        value to put in for the BearerToken authentication header to make an HTTP request to a Deploy endpoint.

    .PARAMETER Action
        Trigger - Trigger Deploy to start extraction. Requires BaseUrl and Reason.
        GetStatus - Retrieves the status of an extraction.
        TriggerWithStatus - Trigger Deploy to start extraction and wait until it succeeds or fails and returns the status response.
        GetToken - Creates the HMAC Authentication token based on the API key that is used in each request.

    .PARAMETER ApiKey
        The Deploy API key.

        For improved security, set the ApiSecret to a cryptographically random value of 64 bytes instead.

    .PARAMETER ApiSecret
        The Deploy API secret as Base64-encoded string.

    .PARAMETER BaseUrl
        The base URL including the scheme, host and port, excluding the trailing slash.

    .PARAMETER Reason
        The reason for extraction: this is used for logging/information.

    .PARAMETER TaskId
        The task ID to get the status for when using GetStatus. If not specified, will get the status from the last/current task.

    .PARAMETER PollingDelaySeconds
        The number of seconds to delay in between polling. Default is 3.

    .EXAMPLE
        .\TriggerDeploy.ps1 -Action Trigger -ApiKey "37D6E975DEF0F6EFED3681A28AA9C49BBF6FB68E" -BaseUrl "http://localhost:45332" -Reason "Test Deploy trigger" -Verbose
        .\TriggerDeploy.ps1 -Action Trigger -ApiSecret "Wz0BhWer7lFSMJHTHfsE56WGh+N/imnmdPeV0XIkIF8YEuJs5PziNjAdIx47Rx3drgA6dPBV2A3ktIELrLubaQ==" -BaseUrl "http://localhost:45332" -Reason "Test Deploy trigger" -Verbose

        Triggers a deployment, ensures any verbose info is printed to the screen and returns the JSON result as a string.

    .EXAMPLE
        .\TriggerDeploy.ps1 -Action Trigger -ApiKey "37D6E975DEF0F6EFED3681A28AA9C49BBF6FB68E" -BaseUrl "http://localhost:45332" -Reason "Test Deploy trigger"
        .\TriggerDeploy.ps1 -Action Trigger -ApiSecret "Wz0BhWer7lFSMJHTHfsE56WGh+N/imnmdPeV0XIkIF8YEuJs5PziNjAdIx47Rx3drgA6dPBV2A3ktIELrLubaQ==" -BaseUrl "http://localhost:45332" -Reason "Test Deploy trigger"

        Triggers a deployment and returns the JSON result as a string.

    .EXAMPLE
        .\TriggerDeploy.ps1 -Action GetToken -ApiKey "37D6E975DEF0F6EFED3681A28AA9C49BBF6FB68E" -Reason "Test Deploy trigger"
        .\TriggerDeploy.ps1 -Action GetToken -ApiSecret "Wz0BhWer7lFSMJHTHfsE56WGh+N/imnmdPeV0XIkIF8YEuJs5PziNjAdIx47Rx3drgA6dPBV2A3ktIELrLubaQ==" -Reason "Test Deploy trigger"

        Returns the authentication token for triggering an extraction.

    .EXAMPLE
        .\TriggerDeploy.ps1 -Action GetStatus -ApiKey "37D6E975DEF0F6EFED3681A28AA9C49BBF6FB68E" -BaseUrl "http://localhost:45332"
        .\TriggerDeploy.ps1 -Action GetStatus -ApiSecret "Wz0BhWer7lFSMJHTHfsE56WGh+N/imnmdPeV0XIkIF8YEuJs5PziNjAdIx47Rx3drgA6dPBV2A3ktIELrLubaQ==" -BaseUrl "http://localhost:45332"

        Returns the status for the current/last triggered extraction task.

    .EXAMPLE
        .\TriggerDeploy.ps1 -Action GetStatus -ApiKey "37D6E975DEF0F6EFED3681A28AA9C49BBF6FB68E" -BaseUrl "http://localhost:45332" -TaskId "8C327019-20BB-4B49-B514-386415648981"
        .\TriggerDeploy.ps1 -Action GetStatus -ApiSecret "Wz0BhWer7lFSMJHTHfsE56WGh+N/imnmdPeV0XIkIF8YEuJs5PziNjAdIx47Rx3drgA6dPBV2A3ktIELrLubaQ==" -BaseUrl "http://localhost:45332" -TaskId "8C327019-20BB-4B49-B514-386415648981"

        Returns the status for the triggered extraction task with the specified ID.
#>
[CmdletBinding(DefaultParameterSetName = "ApiKey")]
param(
    [Parameter(Mandatory, ParameterSetName = "ApiKey")]
    [Parameter(Mandatory, ParameterSetName = "ApiSecret")]
    [ValidateSet("Trigger", "GetStatus", "TriggerWithStatus", "GetToken")]
    [string]
    $Action,

    [Parameter(Mandatory, ParameterSetName = "ApiKey")]
    [string]
    $ApiKey,

    [Parameter(Mandatory, ParameterSetName = "ApiSecret")]
    [string]
    $ApiSecret,

    [Parameter(ParameterSetName = "ApiKey")]
    [Parameter(ParameterSetName = "ApiSecret")]
    [string]
    $BaseUrl,

    [Parameter(ParameterSetName = "ApiKey")]
    [Parameter(ParameterSetName = "ApiSecret")]
    [string]
    $Reason,

    [Parameter(ParameterSetName = "ApiKey")]
    [Parameter(ParameterSetName = "ApiSecret")]
    [string]
    $TaskId,

    [Parameter(ParameterSetName = "ApiKey")]
    [Parameter(ParameterSetName = "ApiSecret")]
    [int]
    $PollingDelaySeconds = 3
)

$ErrorActionPreference = "Stop"

function Get-UnixTimestamp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [DateTime]
        $Timestamp
    )

    $utc = [TimeZoneInfo]::ConvertTimeToUtc($Timestamp)
    $utcBase = New-Object DateTime 1970, 1, 1, 0, 0, 0, ([DateTimeKind]::Utc)

    return ($utc - $utcBase).TotalSeconds
}

function Get-Signature {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]
        $RequestUri,
        [Parameter(Mandatory)]
        [DateTime]
        $Timestamp,
        [Parameter(Mandatory)]
        [string]
        $Nonce
    )

    $unixTimestamp = Get-UnixTimestamp -Timestamp $Timestamp
    $signature = "$RequestUri$unixTimestamp$Nonce"
    $signatureBytes = [Text.Encoding]::UTF8.GetBytes($signature)

    # Parse API key/secret based on used parameter set
    if ($ApiKey) {
        $secretBytes = [Text.Encoding]::UTF8.GetBytes($ApiKey)
    } else {
        $secretBytes = [Convert]::FromBase64String($ApiSecret)
    }

    $hmacsha = New-Object System.Security.Cryptography.HMACSHA256
    $hmacsha.Key = $secretBytes
    $computedBytes = $hmacsha.ComputeHash($signatureBytes)
    $computed = [Convert]::ToBase64String($computedBytes)

    return $computed
}

function Get-AuthorizationHeader {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]
        $Signature,
        [Parameter(Mandatory)]
        [DateTime]
        $Timestamp,
        [Parameter(Mandatory)]
        [string]
        $Nonce
    )

    $unixTimestamp = Get-UnixTimestamp -Timestamp $Timestamp
    $token = "${Signature}:${Nonce}:${unixTimestamp}"
    $tokenBytes = [Text.Encoding]::UTF8.GetBytes($token)
    $encoded = [Convert]::ToBase64String($tokenBytes)
    
    return $encoded
}

function Send-Request {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]
        $Token,
        [Parameter(Mandatory)]
        [string]
        $Endpoint,
        [Parameter(Mandatory)]
        [string]
        $BaseUrl,
        [Parameter(Mandatory)]
        [string]
        $Action
    )

    $uri = "${BaseUrl}${Endpoint}"

    Write-Verbose "Sending request to $uri"

    # Powershell is supposed to support the Authentication parameter for Invoke-WebRequest but it doesn't until later versions
    $headers = @{
        Authorization = "Bearer $Token"
    }

    $response = Invoke-WebRequest -Uri $uri -Headers $headers -ContentType "application/json" -Method $Action

    Write-Verbose $response

    if ($response.StatusCode -ne 200) {
        throw "Cannot continue: the request failed." + @({},{ Use -Verbose flag for more info.})[$VerbosePreference -eq 'SilentlyContinue']
    }

    return $response
}

function Get-ExtractionResult {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [int]
        $PollingDelaySeconds,
        [Parameter(Mandatory)]
        [string]
        $Token,
        [Parameter(Mandatory)]
        [string]
        $Endpoint,
        [Parameter(Mandatory)]
        [string]
        $BaseUrl,
        [Parameter(Mandatory)]
        $Json
    )

    while ($Json.Status -eq "Executing" -or $Json.Status -eq "New") {
        Write-Verbose "Still in progress..."

        Start-Sleep -Seconds $PollingDelaySeconds

        $response = Send-Request -Token $Token -BaseUrl $BaseUrl -Endpoint $Endpoint -Action Get
        $Json = ConvertFrom-Json -InputObject $response.Content
    }

    return $Json
}

function Get-RequestParameters {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]
        $Endpoint
    )

    $now = Get-Date
    $nonce = New-Guid
    $signature = Get-Signature -RequestUri $Endpoint -Timestamp $now -Nonce $nonce
    $token = Get-AuthorizationHeader -Signature $signature -Timestamp $now -Nonce $nonce

    return @{
        Signature = $signature
        Token = $token
        Endpoint = $Endpoint
    }
}

if ($Action -eq "GetToken" -or $Action -eq "Trigger" -or $Action -eq "TriggerWithStatus") {
    if ([string]::IsNullOrEmpty($Reason)) {
        throw "Reason cannot be null or empty."
    }

    $Reason = [Uri]::EscapeDataString($Reason)

    $requestParams = Get-RequestParameters -Endpoint "/umbraco/umbracodeploy/extract/start/$Reason"
}

if ($Action -eq "GetToken") {
    return $requestParams.Token
} else {
    if ([string]::IsNullOrEmpty($BaseUrl)) {
        throw "BaseUrl cannot be null or empty."
    }

    if ($Action -eq "Trigger" -or $Action -eq "TriggerWithStatus") {
        $response = Send-Request -Token $requestParams.Token -BaseUrl $BaseUrl -Endpoint $requestParams.Endpoint -Action Post
        $json = ConvertFrom-Json -InputObject $response.Content
        $TaskId = $json.TaskId # The task ID result

        if ($Action -eq "Trigger") {
            return $response.ToString()
        }
    }

    if ($Action -eq "GetStatus" -or $Action -eq "TriggerWithStatus") {
        if ([string]::IsNullOrEmpty($TaskId)) {
            $requestParams = Get-RequestParameters -Endpoint "/umbraco/umbracodeploy/statusreport/getcurrent"
        } else {
            $requestParams = Get-RequestParameters -Endpoint "/umbraco/umbracodeploy/statusreport/get/$TaskId"
        }

        $response = Send-Request -Token $requestParams.Token -BaseUrl $BaseUrl -Endpoint $requestParams.Endpoint -Action Get
        $json = ConvertFrom-Json -InputObject $response.Content

        if ($Action -eq "GetStatus") {
            return $json
        } else {
            $json = Get-ExtractionResult -Token $requestParams.Token -Endpoint $requestParams.Endpoint -BaseUrl $BaseUrl -Json $json -PollingDelaySeconds $PollingDelaySeconds

            # If the response is unknown, it most likely means that the app restarted after we first initialized the extraction,
            # in which case the status gets removed from memory, so we'll retry the whole thing.
            if ($json.Status -eq "Unknown") {
                Write-Verbose "Status result is Unknown, retrying Extraction again..."

                Start-Sleep -Seconds $PollingDelaySeconds

                # Send the extraction again
                $requestParams = Get-RequestParameters -Endpoint "/umbraco/umbracodeploy/extract/start/$Reason"
                $response = Send-Request -Token $requestParams.Token -BaseUrl $BaseUrl -Endpoint $requestParams.Endpoint -Action Post
                $json = ConvertFrom-Json -InputObject $response.Content
                $TaskId = $json.TaskId # The task ID result

                # Update the values to poll again
                $requestParams = Get-RequestParameters -Endpoint "/umbraco/umbracodeploy/statusreport/get/$TaskId"
                $response = Send-Request -Token $requestParams.Token -BaseUrl $BaseUrl -Endpoint $requestParams.Endpoint -Action Get
                $json = ConvertFrom-Json -InputObject $response.Content
                $json = Get-ExtractionResult -Token $requestParams.Token -Endpoint $requestParams.Endpoint -BaseUrl $BaseUrl -Json $json -PollingDelaySeconds $PollingDelaySeconds
            }

            if ($json.Status -ne "Completed") {
                $err = $json.Status
                if (-not ([string]::IsNullOrEmpty($json.Log))) {
                    $err = $json.Log
                } elseif ($json.Exception) {
                    $err = $json.Exception
                }

                throw "Deploy extraction failed. Response: $($err)"
            }

            return $json
        }
    }
}
