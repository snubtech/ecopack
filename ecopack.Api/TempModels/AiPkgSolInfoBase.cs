using System;
using System.Collections.Generic;

namespace ecopack.Api.TempModels;

/// <summary>
/// AI패키지솔루션정보기본
/// </summary>
public partial class AiPkgSolInfoBase
{
    /// <summary>
    /// 포장재솔루션의 차수이다. 차수는 1차/2차/3차로 구분된다.
    /// </summary>
    public string PkgMatSlnSeq { get; set; } = null!;

    /// <summary>
    /// 솔루션영역(Safety, Reuse, Reduce, Recycle, Replace, Innovation)을 포함하는 값이다.
    /// </summary>
    public string SlnAreaVal { get; set; } = null!;

    /// <summary>
    /// 솔루션영역 별 질문을 구분하는 값이다.
    /// </summary>
    public string SlnDiv { get; set; } = null!;

    /// <summary>
    /// 솔루션영역 별 질문항목이다.
    /// </summary>
    public string? QstnEvlItemCntn { get; set; }

    /// <summary>
    /// 각 질문항목 별 해결 솔루션이다.
    /// </summary>
    public string? RslvSlnCntn { get; set; }

    /// <summary>
    /// 해결 솔루션에 대한 관련 법령 및 규제 상세 설명이다.
    /// </summary>
    public string? RelLawRegDtlExplCntn { get; set; }
}
