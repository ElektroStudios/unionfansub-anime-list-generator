Public Module StringExtensions

    ''' <summary>
    ''' Remove diacritics from the specified string.
    ''' </summary>
    ''' <param name="character">The source string.</param>
    ''' <returns>The resulting string without diacritics.</returns>
    <Extension>
    Public Function RemoveDiacritics(ByVal value As String) As String
        If Not String.IsNullOrWhiteSpace(value) Then
            Dim stFormD As String = value.Normalize(NormalizationForm.FormD)
            Dim sb As New StringBuilder()

            For i As Integer = 0 To (stFormD.Length - 1)
                Dim uc As UnicodeCategory = CharUnicodeInfo.GetUnicodeCategory(stFormD.Chars(i))
                If (uc <> UnicodeCategory.NonSpacingMark) Then
                    sb.Append(stFormD.Chars(i))
                End If
            Next i

            Return sb.ToString().Normalize(NormalizationForm.FormC)
        Else
            Return value
        End If
    End Function

End Module
