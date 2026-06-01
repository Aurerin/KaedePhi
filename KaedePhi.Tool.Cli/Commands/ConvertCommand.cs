using KaedePhi.Tool.Cli.Infrastructure;
using KaedePhi.Tool.Cli.Settings;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter.PhiEdit.Model;
using KaedePhi.Tool.Converter.Phigros.v3.Model;

namespace KaedePhi.Tool.Cli.Commands;

public sealed class ConvertCommand : AsyncCommand<ConvertCommand.Settings>
{
    public sealed class Settings : OperationSettings
    {
        [CommandOption("--target <TYPE>")]
        [LocalizedDescription("convert_command_opt_target")]
        public ChartType? TargetType { get; set; }

        // ---- PhiEdit 转换选项 ----

        [CommandOption("--pe-speed-ratio <N>")]
        [LocalizedDescription("convert_opt_pe_speed_ratio")]
        public double? PeSpeedConversionRatio { get; set; }

        [CommandOption("--pe-trailing-padding <N>")]
        [LocalizedDescription("convert_opt_pe_trailing_padding")]
        public double? PeTrailingBeatPadding { get; set; }

        [CommandOption("--pe-easing-precision <N>")]
        [LocalizedDescription("convert_opt_pe_easing_precision")]
        public double? PeUnsupportedEasingPrecision { get; set; }

        [CommandOption("--pe-xy-precision <N>")]
        [LocalizedDescription("convert_opt_pe_xy_precision")]
        public double? PeMisalignedXyEventPrecision { get; set; }

        [CommandOption("--pe-alpha-precision <N>")]
        [LocalizedDescription("convert_opt_pe_alpha_precision")]
        public double? PeAlphaCutPrecision { get; set; }

        [CommandOption("--pe-alpha-tolerance <N>")]
        [LocalizedDescription("convert_opt_pe_alpha_tolerance")]
        public double? PeAlphaCutTolerance { get; set; }

        [CommandOption("--pe-speed-precision <N>")]
        [LocalizedDescription("convert_opt_pe_speed_precision")]
        public double? PeSpeedCutPrecision { get; set; }

        [CommandOption("--pe-speed-tolerance <N>")]
        [LocalizedDescription("convert_opt_pe_speed_tolerance")]
        public double? PeSpeedCutTolerance { get; set; }

        // ---- PhigrosV3 转换选项 ----

        [CommandOption("--phigros-bpm <N>")]
        [LocalizedDescription("convert_opt_phigros_bpm")]
        public float? PhigrosDefaultBpm { get; set; }

        [CommandOption("--phigros-easing-precision <N>")]
        [LocalizedDescription("convert_opt_phigros_easing_precision")]
        public double? PhigrosEasingPrecision { get; set; }

        [CommandOption("--phigros-xy-precision <N>")]
        [LocalizedDescription("convert_opt_phigros_xy_precision")]
        public double? PhigrosMisalignedXyEventPrecision { get; set; }

        [CommandOption("--phigros-alpha-precision <N>")]
        [LocalizedDescription("convert_opt_phigros_alpha_precision")]
        public double? PhigrosAlphaCutPrecision { get; set; }

        [CommandOption("--phigros-alpha-tolerance <N>")]
        [LocalizedDescription("convert_opt_phigros_alpha_tolerance")]
        public double? PhigrosAlphaCutTolerance { get; set; }

        [CommandOption("--phigros-speed-precision <N>")]
        [LocalizedDescription("convert_opt_phigros_speed_precision")]
        public double? PhigrosSpeedCutPrecision { get; set; }

        // ---- 解绑选项 ----

        [CommandOption("--unbind-precision <N>")]
        [LocalizedDescription("convert_opt_unbind_precision")]
        public double? UnbindPrecision { get; set; }

        [CommandOption("--unbind-tolerance <N>")]
        [LocalizedDescription("convert_opt_unbind_tolerance")]
        public double? UnbindTolerance { get; set; }

        [CommandOption("--unbind-classic")]
        [LocalizedDescription("convert_opt_unbind_classic")]
        public bool? UnbindClassicMode { get; set; }

        // ---- 多层级合并选项 ----

        [CommandOption("--merge-precision <N>")]
        [LocalizedDescription("convert_opt_merge_precision")]
        public double? MultiLayerMergePrecision { get; set; }

        [CommandOption("--merge-tolerance <N>")]
        [LocalizedDescription("convert_opt_merge_tolerance")]
        public double? MultiLayerMergeTolerance { get; set; }

        [CommandOption("--merge-classic")]
        [LocalizedDescription("convert_opt_merge_classic")]
        public bool? MultiLayerMergeClassicMode { get; set; }

        // ---- 压缩控制选项 ----

        [CommandOption("--no-unbind-compress")]
        [LocalizedDescription("convert_opt_no_unbind_compress")]
        public bool? DisableUnbindCompress { get; set; }

        [CommandOption("--no-merge-compress")]
        [LocalizedDescription("convert_opt_no_merge_compress")]
        public bool? DisableMergeCompress { get; set; }

        // ---- 判定线过滤选项 ----

        [CommandOption("--remove-attach-ui")]
        [LocalizedDescription("convert_opt_remove_attach_ui")]
        public bool? RemoveAttachUiLine { get; set; }

        [CommandOption("--remove-texture")]
        [LocalizedDescription("convert_opt_remove_texture")]
        public bool? RemoveTextureLine { get; set; }

        // ---- 音符过滤选项 ----

        [CommandOption("--filter-fake-notes")]
        [LocalizedDescription("convert_opt_filter_fake_notes")]
        public bool? FilterFakeNotes { get; set; }

        // ---- 负透明度抬高选项 ----

        [CommandOption("--negative-alpha-elevation")]
        [LocalizedDescription("convert_opt_negative_alpha_elevation")]
        public bool? NegativeAlphaElevation { get; set; }

        [CommandOption("--negative-alpha-step <N>")]
        [LocalizedDescription("convert_opt_negative_alpha_step")]
        public double? NegativeAlphaStep { get; set; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings s, CancellationToken cancellationToken)
    {
        var c = s.AppConfig.ConvertConfig;
        s.TargetType ??= c.TargetType;
        s.StreamOutput ??= c.StreamOutput;
        s.FormatOutput ??= c.FormatOutput;
        s.DryRun ??= c.DryRun;

        var svc = new ChartService();

        var kpc = await svc.LoadKpcAsync(s.Input, s.Workspace, cancellationToken);
        if (kpc == null) { ConsoleWriter.Error(CliLocalizationString.err_unimplemented); return 1; }

        var output = svc.ResolveOutputPath(s.Input, s.Output, s.Workspace);

        var disableUnbindCompress = s.DisableUnbindCompress ?? false;
        var disableMergeCompress = s.DisableMergeCompress ?? false;

        var peOptions = new KpcToPhiEditConvertOptions
        {
            SpeedConversionRatio = s.PeSpeedConversionRatio ?? c.PeSpeedConversionRatio,
            TrailingBeatPadding = s.PeTrailingBeatPadding ?? c.PeTrailingBeatPadding,
            Cutting = new KpcToPhiEditConvertOptions.CuttingOptions
            {
                UnsupportedEasingPrecision = s.PeUnsupportedEasingPrecision ?? c.PeUnsupportedEasingPrecision,
                MisalignedXyEventPrecision = s.PeMisalignedXyEventPrecision ?? c.PeMisalignedXyEventPrecision
            },
            Alpha = new KpcToPhiEditConvertOptions.AlphaOptions
            {
                CutPrecision = s.PeAlphaCutPrecision ?? c.PeAlphaCutPrecision,
                CutCompress = c.PeAlphaCutCompress,
                CutTolerance = s.PeAlphaCutTolerance ?? c.PeAlphaCutTolerance
            },
            Speed = new KpcToPhiEditConvertOptions.SpeedOptions
            {
                CutPrecision = s.PeSpeedCutPrecision ?? c.PeSpeedCutPrecision,
                CutCompress = c.PeSpeedCutCompress,
                CutTolerance = s.PeSpeedCutTolerance ?? c.PeSpeedCutTolerance
            },
            FatherLineUnbind = new KpcToPhiEditConvertOptions.FatherLineUnbindOptions
            {
                Precision = s.UnbindPrecision ?? s.Precision ?? c.UnbindPrecision,
                Tolerance = s.UnbindTolerance ?? s.Tolerance ?? c.UnbindTolerance,
                ClassicMode = s.UnbindClassicMode ?? s.Classic ?? c.UnbindClassicMode,
                Compress = !disableUnbindCompress
            },
            MultiLayerMerge = new KpcToPhiEditConvertOptions.MultiLayerMergeOptions
            {
                Precision = s.MultiLayerMergePrecision ?? c.MultiLayerMergePrecision,
                Tolerance = s.MultiLayerMergeTolerance ?? c.MultiLayerMergeTolerance,
                ClassicMode = s.MultiLayerMergeClassicMode ?? c.MultiLayerMergeClassicMode,
                Compress = !disableMergeCompress
            },
            LineFilter = new KpcToPhiEditConvertOptions.LineFilterOptions
            {
                RemoveAttachUiLine = s.RemoveAttachUiLine ?? false,
                RemoveTextureLine = s.RemoveTextureLine ?? false
            }
        };

        var phigrosOptions = new KpcToPhigrosV3ConvertOptions
        {
            DefaultBpm = s.PhigrosDefaultBpm ?? c.PhigrosDefaultBpm,
            Cutting = new KpcToPhigrosV3ConvertOptions.CuttingOptions
            {
                EasingPrecision = s.PhigrosEasingPrecision ?? c.PhigrosEasingPrecision,
                MisalignedXyEventPrecision = s.PhigrosMisalignedXyEventPrecision ?? c.PhigrosMisalignedXyEventPrecision
            },
            Alpha = new KpcToPhigrosV3ConvertOptions.AlphaOptions
            {
                CutPrecision = s.PhigrosAlphaCutPrecision ?? c.PhigrosAlphaCutPrecision,
                CutCompress = c.PhigrosAlphaCutCompress,
                CutTolerance = s.PhigrosAlphaCutTolerance ?? c.PhigrosAlphaCutTolerance
            },
            Speed = new KpcToPhigrosV3ConvertOptions.SpeedOptions
            {
                CutPrecision = s.PhigrosSpeedCutPrecision ?? c.PhigrosSpeedCutPrecision
            },
            FatherLineUnbind = new KpcToPhigrosV3ConvertOptions.FatherLineUnbindOptions
            {
                Precision = s.UnbindPrecision ?? s.Precision ?? c.UnbindPrecision,
                Tolerance = s.UnbindTolerance ?? s.Tolerance ?? c.UnbindTolerance,
                ClassicMode = s.UnbindClassicMode ?? s.Classic ?? c.UnbindClassicMode,
                Compress = !disableUnbindCompress
            },
            MultiLayerMerge = new KpcToPhigrosV3ConvertOptions.MultiLayerMergeOptions
            {
                Precision = s.MultiLayerMergePrecision ?? c.MultiLayerMergePrecision,
                Tolerance = s.MultiLayerMergeTolerance ?? c.MultiLayerMergeTolerance,
                ClassicMode = s.MultiLayerMergeClassicMode ?? c.MultiLayerMergeClassicMode,
                Compress = !disableMergeCompress
            },
            LineFilter = new KpcToPhigrosV3ConvertOptions.LineFilterOptions
            {
                RemoveAttachUiLine = s.RemoveAttachUiLine ?? false,
                RemoveTextureLine = s.RemoveTextureLine ?? false
            },
            NoteFilter = new KpcToPhigrosV3ConvertOptions.NoteFilterOptions
            {
                FilterFakeNotes = s.FilterFakeNotes ?? c.PhigrosFilterFakeNotes
            },
            NegativeAlpha = new KpcToPhigrosV3ConvertOptions.NegativeAlphaOptions
            {
                Enabled = s.NegativeAlphaElevation ?? c.PhigrosNegativeAlphaElevation,
                ElevationStep = s.NegativeAlphaStep ?? c.PhigrosNegativeAlphaStep
            }
        };

        var result = await ChartService.SaveAsAsync(kpc, output, s.TargetType ?? ChartType.RePhiEdit,
            new SaveAsOptions
            {
                Stream = s.StreamOutput ?? false,
                Format = s.FormatOutput ?? false,
                DryRun = s.DryRun ?? false,
                PhiEditOptions = peOptions,
                PhigrosOptions = phigrosOptions
            }, cancellationToken);

        if (result == null) { ConsoleWriter.Warn(CliLocalizationString.warn_rpe_convert); return 2; }
        ConsoleWriter.Info(string.Format(CliLocalizationString.msg_written, result));
        return 0;
    }
}
